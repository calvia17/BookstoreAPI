using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Objects;
using System.Text.RegularExpressions;

namespace RabbitHoleService.Models
{
    /// <summary>
    /// The book store database context.
    /// </summary>
    public partial class BookStoreContext : IdentityDbContext<ApplicationUser>
    {
        public BookStoreContext(DbContextOptions<BookStoreContext> options)
        : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the books.
        /// </summary>
        public DbSet<Book> Books { get; set; }

        /// <summary>
        /// Gets or sets the customers.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        /// <summary>
        /// Gets or sets the staff.
        /// </summary>
        public DbSet<Staff> Staff { get; set; }

        /// <summary>
        /// Gets or sets the orders.
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// Gets or sets the genres.
        /// </summary>
        public DbSet<Genre> Genres { get; set; }

        /// <summary>
        /// Gets or sets the refresh tokens.
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.PhoneNumber).IsUnique().HasFilter("[IsDeleted] = 0");
            modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.Email).IsUnique().HasFilter("[IsDeleted] = 0");
            modelBuilder.Entity<ApplicationUser>().Property(u => u.IsDeleted).IsRequired();

            modelBuilder.Entity<Book>().HasKey(b => b.Id);
            modelBuilder.Entity<Book>().Property(b => b.Name).HasMaxLength(250).IsRequired();
            modelBuilder.Entity<Book>().HasIndex(b => b.Name).HasDatabaseName("Books_Name");
            modelBuilder.Entity<Book>().Property(b => b.Isbn).HasMaxLength(13).IsRequired();
            modelBuilder.Entity<Book>().HasIndex(b => b.Isbn).IsUnique().HasDatabaseName("Books_Isbn");
            modelBuilder.Entity<Book>().Property(b => b.Author).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<Book>().HasIndex(b => b.Author).HasDatabaseName("Books_Author");
            modelBuilder.Entity<Book>().Property(b => b.Cost).HasPrecision(10, 2).IsRequired();
            modelBuilder.Entity<Book>().Property(b => b.Stock).IsRequired();
            modelBuilder.Entity<Book>().Property(b => b.IsDeleted).IsRequired();
            modelBuilder.Entity<Book>().Property(b => b.LastModified).IsRequired();
            modelBuilder.Entity<Book>().HasIndex(b => b.LastModified).HasDatabaseName("Books_LastModified");

            modelBuilder.Entity<Customer>().HasKey(c => c.Id);
            modelBuilder.Entity<Customer>().HasOne(c => c.User).WithOne().HasForeignKey<Customer>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Customer>().HasIndex(c => c.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL").HasDatabaseName("Customers_UserId");
            modelBuilder.Entity<Customer>().Property(c => c.Name).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<Customer>().HasIndex(c => c.Name).HasDatabaseName("Customers_Name");
            modelBuilder.Entity<Customer>().Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<Customer>().HasIndex(c => c.PhoneNumber).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("Customers_PhoneNumber");
            modelBuilder.Entity<Customer>().Property(c => c.Email).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("Customers_Email");
            modelBuilder.Entity<Customer>().Property(c => c.IsDeleted).IsRequired();

            modelBuilder.Entity<Staff>().HasKey(c => c.Id);
            modelBuilder.Entity<Staff>().HasOne(c => c.User).WithOne().HasForeignKey<Staff>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Staff>().HasIndex(c => c.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL").HasDatabaseName("Staff_UserId");
            modelBuilder.Entity<Staff>().Property(c => c.Name).HasMaxLength(200).IsRequired();
            modelBuilder.Entity<Staff>().HasIndex(c => c.Name).HasDatabaseName("Staff_Name");
            modelBuilder.Entity<Staff>().Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired();
            modelBuilder.Entity<Staff>().HasIndex(c => c.PhoneNumber).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("Staff_PhoneNumber");
            modelBuilder.Entity<Staff>().Property(c => c.Email).HasMaxLength(255).IsRequired();
            modelBuilder.Entity<Staff>().HasIndex(c => c.Email).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("Staff_Email");
            modelBuilder.Entity<Staff>().Property(c => c.Salary).HasPrecision(18, 2).IsRequired();
            modelBuilder.Entity<Staff>().Property(c => c.IsDeleted).IsRequired();

            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<Order>().Property(o => o.IdempotencyKey).IsRequired();
            modelBuilder.Entity<Order>().HasIndex(o => o.IdempotencyKey).IsUnique().HasDatabaseName("Orders_IdempotencyKey");
            modelBuilder.Entity<Order>().Property(o => o.CreatedAt).IsRequired();
            modelBuilder.Entity<Order>().Property(o => o.TotalCost).HasPrecision(18, 2).IsRequired();
            modelBuilder.Entity<Order>().Property(o => o.Status).IsRequired();
            modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId).IsRequired();
            modelBuilder.Entity<Order>().HasIndex(o => o.CustomerId).HasDatabaseName("Orders_CustomerId");

            modelBuilder.Entity<Genre>().HasKey(g => g.Id);
            modelBuilder.Entity<Genre>().Property(g => g.Name).IsRequired();

            modelBuilder.Entity<BookGenre>().HasKey(bg => new { bg.BookId, bg.GenreId });
            modelBuilder.Entity<BookGenre>().HasOne(bg => bg.Book).WithMany(b => b.BookGenres).HasForeignKey(bg => bg.BookId);
            modelBuilder.Entity<BookGenre>().HasOne(bg => bg.Genre).WithMany(g => g.BookGenres).HasForeignKey(bg => bg.GenreId);

            modelBuilder.Entity<BookOrder>().HasKey(bo => new { bo.BookId, bo.OrderId });
            modelBuilder.Entity<BookOrder>().Property(bo => bo.Quantity).IsRequired();
            modelBuilder.Entity<BookOrder>().Property(bo => bo.PriceAtPurchase).HasPrecision(18, 2).IsRequired();
            modelBuilder.Entity<BookOrder>().HasOne(bo => bo.Book).WithMany(b => b.BookOrders).HasForeignKey(bo => bo.BookId);
            modelBuilder.Entity<BookOrder>().HasOne(bo => bo.Order).WithMany(o => o.BookOrders).HasForeignKey(bo => bo.OrderId);

            modelBuilder.Entity<RefreshToken>().HasKey(rt => rt.Id);
            modelBuilder.Entity<RefreshToken>().Property(rt => rt.FamilyId).IsRequired();
            modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.FamilyId).HasDatabaseName("RefreshTokens_FamilyId");
            modelBuilder.Entity<RefreshToken>().Property(rt => rt.JwtId).IsRequired();
            modelBuilder.Entity<RefreshToken>().Property(rt => rt.Token).HasMaxLength(64).IsRequired().UseCollation("Latin1_General_CS_AS");
            modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.Token).IsUnique().HasDatabaseName("RefreshTokens_Token");
            modelBuilder.Entity<RefreshToken>().Property(rt => rt.ExpiryDate).IsRequired();
            modelBuilder.Entity<RefreshToken>().HasOne(rt => rt.User).WithMany().HasForeignKey(rt => rt.UserId).IsRequired();
            modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.UserId);
            modelBuilder.Entity<RefreshToken>().Property(rt => rt.IsUsed).IsRequired();

            // Seed the genre data from enum GenreType
            var genreSeeds = Enum.GetValues(typeof(GenreType))
                .Cast<GenreType>()
                .Select(e => new Genre(e, PascalCaseRegex().Replace(e.ToString(), "$1 $2")))
                .ToList();

            modelBuilder.Entity<Genre>().HasData(genreSeeds);
        }

        [GeneratedRegex("([a-z])([A-Z])")]
        private static partial Regex PascalCaseRegex();
    }
}