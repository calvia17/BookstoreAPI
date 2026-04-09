using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The customer dto.
    /// </summary>
    public class CustomerDto
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        public required string PhoneNumber { get; init; }

        /// <summary>
        /// Gets the email.
        /// </summary>
        public required string Email { get; init; }
    }
}
