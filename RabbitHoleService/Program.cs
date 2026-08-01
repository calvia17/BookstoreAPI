using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitHoleService;
using RabbitHoleService.Data;
using RabbitHoleService.Middleware;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;
using RabbitHoleService.Services;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"];

if (string.IsNullOrWhiteSpace(key))
{
    throw new InvalidOperationException("JWT signing key is not configured. Please set 'Jwt:Key' in your configuration.");
}

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Bookstore API", 
        Version = "v1",
        Description = "A RESTful API for managing a bookstore's catalog, orders, customers, and staff.\n\n" +
              "Highlights:\n" +
              "- JWT authentication & role-based authorization\n" +
              "- Refresh token rotation with reuse detection\n" +
              "- Access token blacklisting via Redis\n" +
              "- Redis-backed output and hybrid caching\n\n" +
              "Follow the white rabbit down into a wonderland of books. 🐇"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<BookStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<BookStoreContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleType.Admin.ToString()));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole(RoleType.Staff.ToString(), RoleType.Admin.ToString()));
});

// Configure Redis for output caching
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "output:";
});

// Configure Redis for L2 caching in HybridCache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "hybrid:";
});

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.NoCache());
    options.AddPolicy("DynamicData", builder => builder.Expire(TimeSpan.FromMinutes(5)).Tag("dynamic_data"));
    options.AddPolicy("StaticData", builder => builder.Expire(TimeSpan.FromDays(30)).Tag("static_data"));
});
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(15), // L1 + L2 cache expiration
        LocalCacheExpiration = TimeSpan.FromMinutes(5) // L1 cache expiration
    };
});

builder.Services.AddScoped<ITokenCacheService, TokenCacheService>();
builder.Services.AddHostedService<RefreshTokenCleanupService>();

builder.Services.AddScoped<Seeder>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBookCacheEvictor, BookCacheEvictor>();

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<IBookRepository>(sp => sp.GetRequiredService<BookRepository>());
builder.Services.AddScoped<ICachedBookRepository>(sp =>
    new CachedBookRepository(
        sp.GetRequiredService<BookRepository>(),
        sp.GetRequiredService<HybridCache>()));

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPersonRepository<Customer>, PersonRepository<Customer>>();

builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IPersonRepository<Staff>, PersonRepository<Staff>>();

builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<GenreRepository>();
builder.Services.AddScoped<IGenreRepository>(sp =>
    new CachedGenreRepository(
        sp.GetRequiredService<GenreRepository>(),
        sp.GetRequiredService<HybridCache>()));

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = false; // Uniqueness set in database context.
    options.Password.RequiredLength = 8;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var adminSeeder = serviceProvider.GetRequiredService<Seeder>();
    await adminSeeder.SeedRolesAsync();
    await adminSeeder.SeedAdminAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthorization();
app.UseOutputCache();
app.MapControllers();
app.Run();