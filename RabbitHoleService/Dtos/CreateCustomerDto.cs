using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The Create Customer dto.
    /// </summary>
    public class CreateCustomerDto
    {
        private string name = string.Empty;
        private string phoneNumber = string.Empty;
        private string email = string.Empty;

        /// <summary>
        /// Gets the name.
        /// </summary>
        [Required(ErrorMessage = "The customer name is required.")]
        [StringLength(200)]
        public required string Name
        { 
            get => this.name; 
            init => this.name = value.Trim(); 
        }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        [Required(ErrorMessage = "The phone number is required.")]
        [StringLength(20)]
        [RegularExpression(@"^\+?\d+$", ErrorMessage = "Invalid phone number format.")]
        public required string PhoneNumber
        {
            get => this.phoneNumber;
            init => this.phoneNumber = value.Trim();
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        [Required(ErrorMessage = "The email is required.")]
        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email
        {
            get => this.email;
            init => this.email = value.Trim();
        }
    }
}
