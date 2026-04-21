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
        /// Registers a new user as an admin.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <returns>A value indicating whether the registration was successful.</returns>
        public async Task<bool> RegisterAdminAsync(RegisterUserDto registerData)
        {
            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var userId = await this.RegisterUserAsync(registerData.Name, registerData.Email, registerData.Password, registerData.PhoneNumber, RoleType.Admin);
                    if (userId == null)
                    {
                        return false;
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Registers a new user as a customer.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <returns>A value indicating whether the registration was successful.</returns>
        public async Task<bool> RegisterCustomerAsync(RegisterUserDto registerData)
        {
            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var userId = await this.RegisterUserAsync(registerData.Name, registerData.Email, registerData.Password, registerData.PhoneNumber, RoleType.Customer);
                    if (userId == null)
                    {
                        // User registration failed.
                        return false;
                    }

                    var customerByPhone = await this.unitOfWork.Customers.FindCustomersAsync(null, registerData.PhoneNumber, null, true);
                    var customerByEmail = await this.unitOfWork.Customers.FindCustomersAsync(null, null, registerData.Email);
                    if (customerByPhone.Count > 1
                        || customerByEmail.Count > 1
                        || (customerByPhone.Count == 1 && customerByEmail.Count == 1 && customerByPhone[0].Id != customerByEmail[0].Id))
                    {
                        // Another customer with the same email or phone number already exists
                        return false;
                    }
                    else if (customerByPhone.Count == 1 && customerByEmail.Count == 1 && customerByPhone[0].Id == customerByEmail[0].Id)
                    {
                        if (string.IsNullOrEmpty(customerByPhone[0].UserId))
                        {
                            // Link existing customer to the new user account
                            customerByPhone[0].UserId = userId;
                        }
                        else
                        {
                            // The customer is already linked to another user account
                            return false;
                        }
                    }
                    else
                    {
                        this.unitOfWork.Customers.Add(new Customer(registerData.Name, registerData.PhoneNumber, registerData.Email) { UserId = userId });
                    }

                    await this.unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
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
        /// Deletes a user account.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The result of the delete operation.</returns>
        public async Task DeleteAccountAsync(string userId)
        {
            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var user = await this.userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return;
                    }

                    user.IsDeleted = true;
                    await userManager.UpdateAsync(user);
                    this.unitOfWork.RefreshTokens.DeleteTokensByUserId(userId);
                    await this.unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
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

        /// <summary>
        /// Registes a new user account.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="role">The role.</param>
        /// <returns>The user id if registration is successful, otherwise null.</returns>
        public async Task<string?> RegisterUserAsync(string name, string email, string password, string phoneNumber, RoleType role)
        {
            var userExists = await this.userManager.FindByNameAsync(email);
            if (userExists != null)
            {
                return null;
            }

            var user = new ApplicationUser(name)
            {
                UserName = email,
                Email = email,
                PhoneNumber = phoneNumber
            };

            var result = await this.userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return null;
            }

            await this.userManager.AddToRoleAsync(user, role.ToString());
            return user.Id;
        }

        /// <summary>
        /// Updates the account details.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="role">The role of the user.</param>
        /// <returns>A value indicating whether the update was successful.</returns>
        public async Task<bool> UpdateAccountAsync(string userId, UpdateContactInfoDto updateData, RoleType role)
        {
            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var user = await this.userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(updateData.Name))
                    {
                        user.Name = updateData.Name;
                    }
                    if (!string.IsNullOrEmpty(updateData.Email))
                    {
                        user.Email = updateData.Email;
                    }
                    if (!string.IsNullOrEmpty(updateData.PhoneNumber))
                    {
                        user.PhoneNumber = updateData.PhoneNumber;
                    }

                    var result = await this.userManager.UpdateAsync(user);

                    if (!result.Succeeded)
                    {
                        return false;
                    }

                    if (role == RoleType.Customer)
                    {
                        var customer = await this.unitOfWork.Customers.GetByUserIdAsync(userId, true);
                        if (customer != null)
                        {
                            this.customerService.UpdateProperties(updateData, customer);
                        }
                    }

                    await this.unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="passwordData">The password data.</param>
        /// <returns>A value indicating whether the password update was successful.</returns>
        public async Task<bool> UpdatePasswordAsync(string userId, PasswordDto passwordData)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!await this.userManager.CheckPasswordAsync(user, passwordData.CurrentPassword))
            {
                return false;
            }

            var result = await this.userManager.ChangePasswordAsync(user, passwordData.CurrentPassword, passwordData.NewPassword);
            return result.Succeeded;
        }
    }
}