using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The update contact information dto.
    /// </summary>
    public class UpdateContactInfoDto
    {
        private string? name;
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
    }
}
