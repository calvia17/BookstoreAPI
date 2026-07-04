namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The registration failed exception.
    /// </summary>
    public class RegistrationFailedException : BaseApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationFailedException" /> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public RegistrationFailedException(string message)
            : base(message)
        {
        }
    }
}
