using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The register user dto.
    /// </summary>
    public class RegisterUserDto : ContactInfoDto
    {
        private string password = string.Empty;

        /// <summary>
        /// Gets the password.
        /// </summary>
        [Required(ErrorMessage = "The password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,64}$", ErrorMessage = "Password must include uppercase letters, lowercase letters, digits, and special characters.")]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 64 characters long.")]
        public required string Password
        {
            get => this.password;
            init => this.password = value.Trim();
        }
    }
}
