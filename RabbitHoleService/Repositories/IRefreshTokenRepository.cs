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
        /// <returns>The refresh token.</returns>
        Task<RefreshToken?> GetByTokenAsync(string token, bool includeUser = false);

        /// <summary>
        /// Creates a new refresh token.
        /// </summary>
        /// <param name="refreshToken">The new refresh token.</param>
        void Add(RefreshToken refreshToken);

        /// <summary>
        /// Deletes a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        void Delete(RefreshToken refreshToken);

        /// <summary>
        /// Deletes the refresh tokens for a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        void DeleteTokensByUserId(string userId);
    }
}
