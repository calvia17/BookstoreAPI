namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The customer not found exception.
    /// </summary>
    public class CustomerNotFoundException : BaseApplicationException
    {
        /// <summary>
        /// Gets the customer id.
        /// </summary>
        public Guid? CustomerId { get; }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public string? UserId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerNotFoundException" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public CustomerNotFoundException(Guid id)
            : base($"The requested customer with ID '{id}' was not found.")
        {
            this.CustomerId = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerNotFoundException" /> class.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public CustomerNotFoundException(string userId)
            : base($"The requested customer with user ID '{userId}' was not found.")
        {
            this.UserId = userId;
        }
    }
}
