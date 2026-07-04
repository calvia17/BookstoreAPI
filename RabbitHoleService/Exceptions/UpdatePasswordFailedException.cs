namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The update password failed exception.
    /// </summary>
    public class UpdatePasswordFailedException : BaseApplicationException
    {
        /// <summary>
        /// Gets the user id.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatePasswordFailedException" /> class.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="message">The error message.</param>
        public UpdatePasswordFailedException(string userId, string message)
            : base(message)
        {
            this.UserId = userId;
        }
    }
}
