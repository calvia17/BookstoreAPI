using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;
using RabbitHoleService.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthenticationResult = RabbitHoleService.Dtos.AuthenticationResult;

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
        /// Registers an admin.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <returns>The result of the registration operation.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("register/admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegisterUserDto registerData)
        {
            var succeeded = await this.authService.RegisterAdminAsync(registerData);
            if (succeeded)
            {
                return Ok();
            }

            return BadRequest(new
            {
                Title = "Registration Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Registration failed."
            });
        }

        /// <summary>
        /// Registers a customer.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <returns>The result of the registration operation.</returns>
        [AllowAnonymous]
        [HttpPost("register/customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterCustomerAsync([FromBody] RegisterUserDto registerData)
        {
            var succeeded = await this.authService.RegisterCustomerAsync(registerData);
            if (succeeded)
            {
                return Ok();
            }

            return BadRequest(new
            {
                Title = "Registration Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Registration failed."
            });
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
        /// Deletes a user account.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the logout operation.</returns>
        [Authorize]
        [HttpDelete("account")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null && Enum.TryParse<RoleType>(User.FindFirstValue(ClaimTypes.Role), true, out var role))
            {
                await this.authService.DeleteAccountAsync(userId, role);
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

        /// <summary>
        /// Updates the account details.
        /// </summary>
        /// <param name="updateData">The update data.</param>
        /// <returns>The result of the update operation.</returns>
        [Authorize]
        [HttpPatch("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAccountAsync([FromBody] UpdateContactInfoDto updateData)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Enum.TryParse<RoleType>(User.FindFirstValue(ClaimTypes.Role), true, out var role))
            {
                return Unauthorized();
            } 

            var succeeded = await this.authService.UpdateAccountAsync(userId, updateData, role);
            if (succeeded)
            {
                return NoContent();
            }

            return BadRequest(new
            {
                Title = "Update Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Update failed."
            });
        }

        /// <summary>
        /// Updates the password for a user.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The result of the change password operation.</returns>
        [Authorize]
        [HttpPatch("me/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePasswordAsync([FromBody] PasswordDto password)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var succeeded = await this.authService.UpdatePasswordAsync(userId, password);
            if (succeeded)
            {
                return NoContent();
            }

            return BadRequest(new
            {
                Title = "Password Update Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Password update failed."
            });
        }
    }
}