using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The refresh token repository interface.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="includeUser">A value indicating whether the user should be included,</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The refresh token.</returns>
        Task<RefreshToken?> GetByTokenAsync(string token, bool includeUser = false, bool trackChanges = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new refresh token.
        /// </summary>
        /// <param name="refreshToken">The new refresh token.</param>
        void Add(RefreshToken refreshToken);

        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> RevokeTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the refresh tokens for a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param>The revoked refresh tokens.</param>
        Task<IEnumerable<RefreshToken>> RevokeTokensByUserId(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the refresh tokens for a family.
        /// </summary>
        /// <param name="familyId">The family id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The revoked refresh tokens.</returns>
        Task<IEnumerable<RefreshToken>> RevokeTokensByFamilyId(Guid familyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the expired refresh tokens.
        /// </summary>
        /// <param name="cutoff">The cutoff.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>A task representing the delete operation.</returns>
        Task DeleteExpiredTokensAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);
    }
}
