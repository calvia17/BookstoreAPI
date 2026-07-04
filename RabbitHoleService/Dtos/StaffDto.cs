namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The staff dto.
    /// </summary>
    public class StaffDto
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

        /// <summary>
        /// Gets the salary.
        /// </summary>
        public decimal Salary { get; init; }
    }
}
