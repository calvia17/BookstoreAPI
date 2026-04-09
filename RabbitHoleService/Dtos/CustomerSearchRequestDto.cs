using RabbitHoleService.Objects;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The customer search request request dto.
    /// </summary>
    public class CustomerSearchRequestDto
    {
        private string? name;
        private string? phoneNumber;
        private string? email;

        /// <summary>
        /// Gets the name.
        /// </summary>
        [StringLength(200, ErrorMessage = "The customer name should not exceed 200 characters.")]
        public string? Name
        {
            get => this.name;
            init => this.name = value?.Trim();
        }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        [StringLength(20)]
        [RegularExpression(@"^\+?\d+$", ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber
        {
            get => this.phoneNumber;
            init => this.phoneNumber = value?.Trim();
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        [StringLength(255)]
        public string? Email
        {
            get => this.email;
            init => this.email = value?.Trim();
        }
    }
}
