using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The person interface.
    /// </summary>
    public interface IPerson
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        Guid Id { get; init; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// A value indicating whether the customer is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The user id.
        /// </summary>
        public string? UserId { get; set; }
    }
}
