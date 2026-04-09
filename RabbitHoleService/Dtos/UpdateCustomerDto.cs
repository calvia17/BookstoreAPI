using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The update customer dto.
    /// </summary>
    public class UpdateCustomerDto
    {
        private string? name;
        private string? email;
        private string? phoneNumber;

        /// <summary>
        /// Gets or sets the name.
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
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email
        {
            get => this.email;
            init => this.email = value?.Trim();
        }
    }
}
