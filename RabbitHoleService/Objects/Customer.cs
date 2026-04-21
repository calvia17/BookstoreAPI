using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The customer class.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public Guid Id { get; init; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public ApplicationUser? User { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required(ErrorMessage = "The customer name is required.")]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [Required(ErrorMessage = "The phone number is required.")]
        [StringLength(20)]
        [RegularExpression(@"^\+?[0-9\s\-\(\)\.]{7,20}$", ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required(ErrorMessage = "The email is required.")]
        [StringLength(255)]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        /// <summary>
        /// Gets the orders
        /// </summary>
        public ICollection<Order> Orders { get; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="Customer" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="email">The email.</param>
        public Customer(string name, string phoneNumber, string email)
        {
            this.Name = name;
            this.PhoneNumber = phoneNumber;
            this.Email = email;
            this.Orders = new List<Order>();
        }
    }
}
