namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The update person failed exception.
    /// </summary>
    public class UpdatePersonFailedException : BaseApplicationException
    {
        /// <summary>
        /// Gets the person id.
        /// </summary>
        public Guid? PersonId { get; }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public string? UserId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersonFailedException" /> class.
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <param name="message">The error message.</param>
        public UpdatePersonFailedException(Guid personId, string message)
            : base(message)
        {
            this.PersonId = personId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePersonFailedException" /> class.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="message">The error message.</param>
        public UpdatePersonFailedException(string userId, string message)
            : base(message)
        {
            this.UserId = userId;
        }
    }
}
