using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Dtos;
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
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The refresh token.</returns>
        public async Task<RefreshToken?> GetByTokenAsync(string token, bool includeUser = false, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(token);
            IQueryable<RefreshToken> query = this.context.RefreshTokens;
            if (!trackChanges)
            {
                query = this.context.RefreshTokens.AsNoTracking();
            }
            if (includeUser)
            {
                query = query.Include(rt => rt.User);
            }

            var refreshToken = await query.FirstOrDefaultAsync(c => c.Token == token, cancellationToken);
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
        /// Revokes a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        public void RevokeToken(RefreshToken refreshToken)
        {
            ArgumentNullException.ThrowIfNull(refreshToken);
            refreshToken.IsUsed = true;
        }

        /// <summary>
        /// Revokes the refresh tokens for a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param>The revoked refresh tokens.</param>
        public async Task<IEnumerable<RefreshToken>> RevokeTokensByUserId(string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            var tokens = await this.context.RefreshTokens.Where(t => t.UserId == userId && !t.IsUsed).ToListAsync(cancellationToken);
            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            return tokens;
        }

        /// <summary>
        /// Revokes the refresh tokens for a family.
        /// </summary>
        /// <param name="familyId">The family id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The revoked refresh tokens.</returns>
        public async Task<IEnumerable<RefreshToken>> RevokeTokensByFamilyId(Guid familyId, CancellationToken cancellationToken = default)
        {
            var tokens = await this.context.RefreshTokens.Where(t => t.FamilyId == familyId && !t.IsUsed).ToListAsync(cancellationToken);
            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            return tokens;
        }

        /// <summary>
        /// Deletes the expired refresh tokens.
        /// </summary>
        /// <param name="cutoff">The cutoff.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>A task representing the delete operation.</returns>
        public async Task DeleteExpiredTokensAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
        {
            await this.context.RefreshTokens.Where(t => t.ExpiryDate <= cutoff).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
