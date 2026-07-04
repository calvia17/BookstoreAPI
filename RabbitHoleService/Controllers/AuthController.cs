using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;
using RabbitHoleService.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RabbitHoleService.Controllers
{
    /// <summary>
    /// The authorization controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        /// <summary>
        /// Initialises the authorization controller.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        /// <summary>
        /// Logs in a user.
        /// </summary>
        /// <param name="loginData">The login data.</param>
        /// <returns>The result of the login operation.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginData)
        {
            AuthenticationResult authResult = await this.authService.LoginAsync(loginData);
            if (authResult.Succeeded)
            {
                return Ok(new 
                { 
                    token = new JwtSecurityTokenHandler().WriteToken(authResult.Tokens!.AccessToken), 
                    refreshToken = authResult.Tokens!.RefreshToken
                });
            }
            else
            {
                if (authResult.ErrorCode == AuthenticationFailure.AccountLocked)
                {
                    return Unauthorized(new
                    {
                        Title = "Account locked",
                        Status = StatusCodes.Status401Unauthorized,
                        Detail = authResult.ErrorDescription
                    });
                }
                else if (authResult.ErrorCode == AuthenticationFailure.InvalidCredentials)
                {
                    return Unauthorized(new
                    {
                        Title = "Invalid Credentials",
                        Status = StatusCodes.Status401Unauthorized,
                        Detail = authResult.ErrorDescription
                    });
                }
            }

            return Unauthorized(new
            {
                Title = "Login Failed",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "An unknown error occurred."
            });
        }

        /// <summary>
        /// Logs out a user.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the logout operation.</returns>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await this.authService.LogoutAsync(userId, refreshToken);
            }

            return Ok();
        }

        /// <summary>
        /// Logs out a user from all sessions.
        /// </summary>
        /// <returns>The result of the logout operation.</returns>
        [Authorize]
        [HttpPost("logout-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LogoutAllSessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await this.authService.LogoutAllSessionsAsync(userId);
            }

            return Ok();
        }

        /// <summary>
        /// Refreshes the token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the token refresh operation.</returns>
        [Authorize]
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var result = await this.authService.RefreshTokenAsync(userId, refreshToken.RefreshToken);
                if (result.Succeeded)
                {
                    return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(result.Tokens!.AccessToken), refreshToken = result.Tokens!.RefreshToken });
                }
            }
            return Unauthorized(new
            {
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Session expired."
            });
        }
    }
}