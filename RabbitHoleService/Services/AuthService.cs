using Azure.Core;
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
        private readonly IUnitOfWork unitOfWork;
        private readonly ITokenCacheService tokenCacheService;

        /// <summary>
        /// Initializes the authentication service.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="tokenCacheService">The token cache service.</param>
        public AuthService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ITokenCacheService tokenCacheService)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.tokenCacheService = tokenCacheService;
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginData">The login data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the login operation.</returns>
        public async Task<AuthenticationResult> LoginAsync(LoginDto loginData, CancellationToken cancellationToken = default)
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

            var jwtId = Guid.NewGuid();
            var accessToken = await GenerateAccessToken(user, jwtId);
            var refreshToken = GenerateRefreshToken();
            var hashedRefreshToken = HashRefreshToken(refreshToken);
            this.unitOfWork.RefreshTokens.Add(new RefreshToken(hashedRefreshToken, user.Id, DateTimeOffset.UtcNow.AddDays(14), Guid.NewGuid(), jwtId));
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
            return new AuthenticationResult(true, null, null, new Tokens(accessTokenString, refreshToken));
        }

        /// <summary>
        /// Logs out a user.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the logout operation.</returns>
        public async Task LogoutAsync(string userId, RefreshTokenDto refreshToken, CancellationToken cancellationToken = default)
        {
            var hashedRefreshToken = HashRefreshToken(refreshToken.RefreshToken);
            var existingToken = await this.unitOfWork.RefreshTokens.GetByTokenAsync(hashedRefreshToken, false, true, cancellationToken: cancellationToken);
            if (existingToken != null && existingToken.UserId == userId)
            {
                existingToken.IsUsed = true;
                this.unitOfWork.RefreshTokens.RevokeToken(existingToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                // Blacklist the associated access token
                var blacklistTokenRequest = new BlacklistTokenRequestDto(existingToken.JwtId, existingToken.ExpiryDate);
                await this.tokenCacheService.BlacklistTokensAsync([blacklistTokenRequest], cancellationToken);
            }
        }

        /// <summary>
        /// Logs out a user from all sessions.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the logout operation.</returns>
        public async Task LogoutAllSessionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var tokens = await this.unitOfWork.RefreshTokens.RevokeTokensByUserId(userId, cancellationToken);
            if (tokens.Any())
            {
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                // Blacklist all associated access tokens
                var blacklistTokenRequests = tokens.Select(token => new BlacklistTokenRequestDto(token.JwtId, token.ExpiryDate));
                await this.tokenCacheService.BlacklistTokensAsync(blacklistTokenRequests, cancellationToken);
            }
        }

        /// <summary>
        /// Refreshes the access and refresh tokens.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the token refresh operation.</returns>
        public async Task<RefreshTokenResult> RefreshTokenAsync(string userId, string refreshToken, CancellationToken cancellationToken = default)
        {
            var hashedRefreshToken = HashRefreshToken(refreshToken);
            var existingToken = await this.unitOfWork.RefreshTokens.GetByTokenAsync(hashedRefreshToken, true, true, cancellationToken);
            if (existingToken != null && existingToken.ExpiryDate > DateTimeOffset.UtcNow)
            {
                var cachedTokens = await this.tokenCacheService.GetCachedTokensForGracePeriodAsync(existingToken.Id, cancellationToken);
                if (cachedTokens != null)
                {
                    return new RefreshTokenResult(true, cachedTokens);
                }
                else if (existingToken.IsUsed)
                {
                    // Breach detected - revoke all refresh tokens in the family and blacklist the associated access tokens
                    var tokens = await this.unitOfWork.RefreshTokens.RevokeTokensByFamilyId(existingToken.FamilyId, cancellationToken);
                    if (tokens.Any())
                    {
                        await this.unitOfWork.SaveChangesAsync(cancellationToken);

                        // Blacklist all associated access tokens
                        var blacklistTokenRequests = tokens.Select(token => new BlacklistTokenRequestDto(token.JwtId, token.ExpiryDate));
                        await this.tokenCacheService.BlacklistTokensAsync(blacklistTokenRequests, cancellationToken);
                    }

                    return new RefreshTokenResult(false, null);
                }
                else if (existingToken.UserId == userId && existingToken.User != null)
                {
                    // Valid refresh token - generate new access and refresh tokens
                    var jwtId = Guid.NewGuid();
                    var accessToken = await GenerateAccessToken(existingToken.User, jwtId);
                    var newRefreshToken = GenerateRefreshToken();
                    var hashedNewRefreshToken = HashRefreshToken(newRefreshToken);
                    this.unitOfWork.RefreshTokens.Add(new RefreshToken(hashedNewRefreshToken, userId, DateTimeOffset.UtcNow.AddDays(14), existingToken.FamilyId, jwtId));
                    this.unitOfWork.RefreshTokens.RevokeToken(existingToken);
                    await this.unitOfWork.SaveChangesAsync(cancellationToken);

                    // Cache the new tokens for a tiny grace period to allow valid concurrent requests to succeed.
                    var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
                    var tokens = new Tokens(accessTokenString, newRefreshToken);
                    await this.tokenCacheService.CacheTokensForGracePeriodAsync(existingToken.Id, tokens, cancellationToken);

                    return new RefreshTokenResult(true, tokens);
                }
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

        private static string HashRefreshToken(string refreshToken)
        {
            var inputBytes = Encoding.UTF8.GetBytes(refreshToken);
            var hashBytes = SHA256.HashData(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private async Task<JwtSecurityToken> GenerateAccessToken(ApplicationUser user, Guid jwtId)
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
                new Claim(JwtRegisteredClaimNames.Jti, jwtId.ToString())
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