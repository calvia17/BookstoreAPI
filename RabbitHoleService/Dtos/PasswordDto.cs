using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The password dto.
    /// </summary>
    public class PasswordDto
    {
        private string currentPassword = string.Empty;
        private string newPassword = string.Empty;

        /// <summary>
        /// Gets or sets the current password.
        /// </summary>
        [Required(ErrorMessage = "The current password is required.")]
        [MaxLength(64, ErrorMessage = "Invalid password.")]
        public required string CurrentPassword
        {
            get => this.currentPassword;
            init => this.currentPassword = value.Trim();
        }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        [Required(ErrorMessage = "The new password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,64}$", ErrorMessage = "Password must include uppercase letters, lowercase letters, digits, and special characters.")]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 64 characters long.")]
        public required string NewPassword
        {
            get => this.newPassword;
            init => this.newPassword = value.Trim();
        }
    }
}
