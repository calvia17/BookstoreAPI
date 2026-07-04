using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The refresh token repository.
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly BookStoreContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenRepository" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RefreshTokenRepository(BookStoreContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="includeUser">A value indicating whether the user should be included.</param>
        /// <returns>The refresh token.</returns>
        public async Task<RefreshToken?> GetByTokenAsync(string token, bool includeUser = false)
        {
            ArgumentNullException.ThrowIfNull(token);
            var query = this.context.RefreshTokens.AsNoTracking();
            if (includeUser)
            {
                query = query.Include(rt => rt.User);
            }

            var refreshToken = await query.FirstOrDefaultAsync(c => c.Token == token);
            return refreshToken;
        }

        /// <summary>
        /// Creates a new refresh token.
        /// </summary>
        /// <param name="refreshToken">The new refresh token.</param>
        public void Add(RefreshToken refreshToken)
        {
            ArgumentNullException.ThrowIfNull(refreshToken);
            this.context.RefreshTokens.Add(refreshToken);
        }

        /// <summary>
        /// Deletes a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        public void Delete(RefreshToken refreshToken)
        {
            ArgumentNullException.ThrowIfNull(refreshToken);
            this.context.RefreshTokens.Remove(refreshToken);
        }

        /// <summary>
        /// Deletes the refresh tokens for a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public void DeleteTokensByUserId(string userId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            var tokens = this.context.RefreshTokens.Where(t => t.UserId == userId).ToList();
            this.context.RefreshTokens.RemoveRange(tokens);
        }
    }
}
