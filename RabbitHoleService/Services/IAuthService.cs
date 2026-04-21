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
        /// Registers a new user as an admin.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <returns>A value indicating whether the registration was successful.</returns>
        Task<bool> RegisterAdminAsync(RegisterUserDto registerData);

        /// <summary>
        /// Registers a new user as a customer.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <returns>A value indicating whether the registration was successful.</returns>
        Task<bool> RegisterCustomerAsync(RegisterUserDto registerData);

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginData">The login data.</param>
        /// <returns>The result of the login operation.</returns>
        Task<AuthenticationResult> LoginAsync(LoginDto loginData);

        /// <summary>
        /// Logs out a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the logout operation.</returns>
        Task LogoutAsync(string userId, RefreshTokenDto refreshToken);

        /// <summary>
        /// Logs out a user from all sessions.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The result of the logout operation.</returns>
        Task LogoutAllSessionsAsync(string userId);

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="role">The role of the user.</param>
        /// <returns>The result of the delete operation.</returns>
        Task DeleteAccountAsync(string userId, RoleType role);

        /// <summary>
        /// Refreshes the access and refresh tokens.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the token refresh operation.</returns>
        Task<RefreshTokenResult> RefreshTokenAsync(string userId, string refreshToken);

        /// <summary>
        /// Updates the account details.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="role">The role of the user.</param>
        /// <returns>A value indicating whether the update was successful.</returns>
        Task<bool> UpdateAccountAsync(string userId, UpdateContactInfoDto updateData, RoleType role);

        
        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="passwordData">The password data.</param>
        /// <returns>A value indicating whether the password update was successful.</returns>
        Task<bool> UpdatePasswordAsync(string userId, PasswordDto passwordData);
    }
}
