using RabbitHoleService.Dtos;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The token cache service interface.
    /// </summary>
    public interface ITokenCacheService
    {
        /// <summary>
        /// Gets the cached tokens for the grace period. This is used to allow concurrent valid requests from a user to not detect a security breach.
        /// </summary>
        /// <param name="oldRefreshTokenId">The old refresh token id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The cached tokens if available, otherwise null.</returns>
        Task<Tokens?> GetCachedTokensForGracePeriodAsync(Guid oldRefreshTokenId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Caches the refresh and access tokens for the grace period so that concurrent valid requests from a user do not detect a security breach.
        /// </summary>
        /// <param name="oldRefreshTokenId">The old refresh token id.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CacheTokensForGracePeriodAsync(Guid oldRefreshTokenId, Tokens tokens, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a token is blacklisted.
        /// </summary>
        /// <param name="jwtId">The JWT ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the token is blacklisted, otherwise false.</returns>
        Task<bool> IsTokenBlacklistedAsync(Guid jwtId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Blacklists access tokens.
        /// </summary>
        /// <param name="tokens">The tokens to blacklist.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task BlacklistTokensAsync(IEnumerable<BlacklistTokenRequestDto> tokens, CancellationToken cancellationToken = default);
    }
}
