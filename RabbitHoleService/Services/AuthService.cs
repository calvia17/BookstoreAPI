using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The authentication service.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICustomerService customerService;
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Initializes the authentication service.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="customerService">The customer service.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        public AuthService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ICustomerService customerService,
            IUnitOfWork unitOfWork)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.customerService = customerService;
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginData">The login data.</param>
        /// <returns>The result of the login operation.</returns>
        public async Task<AuthenticationResult> LoginAsync(LoginDto loginData)
        {
            var user = await this.userManager.FindByNameAsync(loginData.Email);
            if (user != null && !user.IsDeleted && user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                return new AuthenticationResult(
                    false,
                    AuthenticationFailure.AccountLocked,
                    "User account is locked. Try again after 15 minutes.",
                    null);
            }

            if (user == null || user.IsDeleted || !await this.userManager.CheckPasswordAsync(user, loginData.Password))
            {
                if (user != null && !user.IsDeleted)
                {
                    await this.userManager.AccessFailedAsync(user);
                }

                return new AuthenticationResult(
                    false,
                    AuthenticationFailure.InvalidCredentials,
                    "Invalid username or password.",
                    null);
            }

            var accesstoken = await GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();
            this.unitOfWork.RefreshTokens.Add(new RefreshToken(refreshToken, user.Id, DateTimeOffset.UtcNow.AddDays(14)));
            await this.unitOfWork.SaveChangesAsync();
            return new AuthenticationResult(true, null, null, new Tokens(accesstoken, refreshToken));
        }

        /// <summary>
        /// Logs out a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the logout operation.</returns>
        public async Task LogoutAsync(string userId, RefreshTokenDto refreshToken)
        {
            var existingToken = await this.unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken.RefreshToken);
            if (existingToken != null && existingToken.UserId == userId)
            {
                this.unitOfWork.RefreshTokens.Delete(existingToken);
                await this.unitOfWork.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Logs out a user from all sessions.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The result of the logout operation.</returns>
        public async Task LogoutAllSessionsAsync(string userId)
        {
            this.unitOfWork.RefreshTokens.DeleteTokensByUserId(userId);
            await this.unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Refreshes the access and refresh tokens.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the token refresh operation.</returns>
        public async Task<RefreshTokenResult> RefreshTokenAsync(string userId, string refreshToken)
        {
            var existingToken = await this.unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken, true);
            if (existingToken != null && existingToken.ExpiryDate > DateTimeOffset.UtcNow && existingToken.UserId == userId && existingToken.User != null)
            {
                var accesstoken = await GenerateAccessToken(existingToken.User);
                var newRefreshToken = GenerateRefreshToken();
                this.unitOfWork.RefreshTokens.Add(new RefreshToken(newRefreshToken, userId, DateTimeOffset.UtcNow.AddDays(14)));
                this.unitOfWork.RefreshTokens.Delete(existingToken);
                await this.unitOfWork.SaveChangesAsync();
                return new RefreshTokenResult(true, new Tokens(accesstoken, newRefreshToken));
            }
            
            return new RefreshTokenResult(false, null);
        }

        private static string GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            string refreshToken = Convert.ToBase64String(randomBytes);
            return refreshToken;
        }

        private async Task<JwtSecurityToken> GenerateAccessToken(ApplicationUser user)
        {
            var jwtSettings = this.configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var userRoles = await this.userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: authClaims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds);
            return token;
        }
    }
}