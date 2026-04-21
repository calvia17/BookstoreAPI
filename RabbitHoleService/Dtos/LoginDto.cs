using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The login dto.
    /// </summary>
    public class LoginDto
    {
        private string email = string.Empty;
        private string password = string.Empty;

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        public required string Email
        {
            get => this.email;
            init => this.email = value.Trim();
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        public required string Password
        {
            get => this.password;
            init => this.password = value.Trim();
        }
    }
}
