using RedisRateLimiting;
using StackExchange.Redis;
using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace RabbitHoleService
{
    /// <summary>
    /// The rate limit configuration.
    /// </summary>
    public static class RateLimitConfiguration
    {
        /// <summary>
        /// Adds the rate limit policies to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    await context.HttpContext.Response.WriteAsJsonAsync("Too many requests. Please try again later.", cancellationToken);
                };

                // This global rate limiter only limits the login endpoint. 
                // It prevents brute-force attacks on the login endpoint by limiting the number of requests from a single IP address.
                // It is currently not possible to chain multiple Redis rate limiters together,
                // so we have to use a global limiter for IP based rate limiting and a separate policy for username based rate limiting.
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    if (!context.Request.Path.Value?.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase) ?? true)
                    {
                        return RateLimitPartition.GetNoLimiter("pass");
                    }

                    var partitionKey = GetPartitionKey(context.Connection.RemoteIpAddress, null, null);
                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(partitionKey, _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                        TokenLimit = 5,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(15),
                    });
                });

                // This policy prevents brute-force attacks on the login endpoint by limiting the number of login requests with the same username.
                options.AddPolicy("AuthLoginPolicy", context =>
                {
                    var partitionKey = GetPartitionKey(null, null, context.Items["LoginEmail"]?.ToString());
                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(partitionKey, _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                        TokenLimit = 5,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(15),
                    });
                });

                options.AddPolicy("ReadPolicy", context =>
                {
                    var partitionKey = context.User?.Identity?.IsAuthenticated == true
                        ? GetPartitionKey(null, context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, null)
                        : GetPartitionKey(context.Connection.RemoteIpAddress, null, null);

                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(partitionKey, _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                        TokenLimit = 60,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(3),
                    });
                });

                options.AddPolicy("AuthPolicy", context =>
                {
                    var partitionKey = GetPartitionKey(null, context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, null);
                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(partitionKey, _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                        TokenLimit = 10,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    });
                });

                options.AddPolicy("CheckoutPolicy", context =>
                {
                    var partitionKey = context.User?.Identity?.IsAuthenticated == true
                        ? GetPartitionKey(null, context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, null)
                        : GetPartitionKey(context.Connection.RemoteIpAddress, null, null);
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(partitionKey, _ => new RedisSlidingWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                        Window = TimeSpan.FromMinutes(2),
                        PermitLimit = 3,
                    });
                });

                options.AddPolicy("BookManagementPolicy", context =>
                {
                    var partitionKey = GetPartitionKey(null, context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, null);
                    return RedisRateLimitPartition.GetTokenBucketRateLimiter(partitionKey, _ => new RedisTokenBucketRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                        TokenLimit = 30,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(2),
                    });
                });

                options.AddPolicy("UserManagementPolicy", context =>
                {
                    var partitionKey = GetPartitionKey(null, context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, null);
                    return RedisRateLimitPartition.GetSlidingWindowRateLimiter(partitionKey, _ => new RedisSlidingWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => context.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
                        Window = TimeSpan.FromMinutes(5),
                        PermitLimit = 20,
                    });
                });
            });

            return services;
        }

        private static string GetPartitionKey(IPAddress? ipAddress, string? username, string? email)
        {
            if (ipAddress != null)
            {
                return $"ip:{ipAddress}";
            }
            else if (!string.IsNullOrEmpty(username))
            {
                return $"username:{username}";
            }
            else if (!string.IsNullOrEmpty(email))
            {
                return $"email:{email}";
            }
            else
            {
                // If no key can be verified, pool all requests into a single partition key to avoid bypassing the rate limit.
                // Otherwise, hackers could hide their IP address to bypass the rate limit.
                return $"unknown";
            }
        }
    }
}
