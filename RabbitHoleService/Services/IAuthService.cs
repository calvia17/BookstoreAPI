using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The authentication interface.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginData">The login data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the login operation.</returns>
        Task<AuthenticationResult> LoginAsync(LoginDto loginData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs out a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the logout operation.</returns>
        Task LogoutAsync(string userId, RefreshTokenDto refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs out a user from all sessions.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the logout operation.</returns>
        Task LogoutAllSessionsAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the access and refresh tokens.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the token refresh operation.</returns>
        Task<RefreshTokenResult> RefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default);
    }
}
