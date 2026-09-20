namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The invalid image exception.
    /// </summary>
    public class InvalidImageException : BaseApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidImageException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidImageException(string message)
            : base(message)
        {
        }
    }
}
