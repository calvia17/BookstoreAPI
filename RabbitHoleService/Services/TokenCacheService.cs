using Microsoft.Extensions.Caching.Distributed;
using RabbitHoleService.Dtos;
using System.Text.Json;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The token cache service.
    /// </summary>
    public class TokenCacheService : ITokenCacheService
    {
        // We use distributed cache (Redis) to store blacklisted tokens instead of hybrid cache.
        // Storing them in hybrid cache would mean that if the application restarts, the blacklisted tokens would be lost.
        // Redis handles this by persisting the data so that it is not lost on application restarts.
        // Use hybrid cache (in-memory + distributed) also has the issue of inconsistency between in memory caches of different servers unless invalidated properly.
        // In this scenario, the tokens are automatically removed by setting the TTL as the remaining time of the token, so there is no need to invalidate.
        // Redis lookups are extremely fast, so the performance impact is negligible.
        private readonly IDistributedCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCacheService" /> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        public TokenCacheService(IDistributedCache cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Gets the cached tokens for the grace period. This is used to allow concurrent valid requests from a user to not detect a security breach.
        /// </summary>
        /// <param name="oldRefreshTokenId">The old refresh token id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The cached tokens if available, otherwise null.</returns>
        public async Task<Tokens?> GetCachedTokensForGracePeriodAsync(Guid oldRefreshTokenId, CancellationToken cancellationToken = default)
        {
            var key = $"tokens:grace:{oldRefreshTokenId}";
            var serializedTokens = await this.cache.GetStringAsync(key, cancellationToken);
            if (serializedTokens == null)
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<Tokens>(serializedTokens);
            }
            catch (JsonException)
            {
                await this.cache.RemoveAsync(key, cancellationToken);
                return null;
            }
        }

        /// <summary>
        /// Caches the refresh and access tokens for the grace period so that concurrent valid requests from a user do not detect a security breach.
        /// </summary>
        /// <param name="oldRefreshTokenId">The old refresh token id.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CacheTokensForGracePeriodAsync(Guid oldRefreshTokenId, Tokens tokens, CancellationToken cancellationToken = default)
        {
            var serializedTokens = JsonSerializer.Serialize(tokens);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            };

            await this.cache.SetStringAsync($"tokens:grace:{oldRefreshTokenId}", serializedTokens, options, cancellationToken);
        }

        /// <summary>
        /// Checks if a token is blacklisted.
        /// </summary>
        /// <param name="jwtId">The JWT ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the token is blacklisted, otherwise false.</returns>
        public async Task<bool> IsTokenBlacklistedAsync(Guid jwtId, CancellationToken cancellationToken = default)
        {
            var token = await this.cache.GetStringAsync($"tokens:blacklist:{jwtId}", cancellationToken);
            return token != null;
        }

        /// <summary>
        /// Blacklists access tokens.
        /// </summary>
        /// <param name="tokens">The tokens to blacklist.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task BlacklistTokensAsync(IEnumerable<BlacklistTokenRequestDto> tokens, CancellationToken cancellationToken = default)
        {
            var currentTime = DateTimeOffset.UtcNow;
            var tasks = tokens.Where(token => token.Expiration > currentTime).Select(token =>
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = token.Expiration
                };

                return this.cache.SetStringAsync($"tokens:blacklist:{token.JwtId}", "revoked", options, cancellationToken);
            }).ToList();

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }
    }
}
