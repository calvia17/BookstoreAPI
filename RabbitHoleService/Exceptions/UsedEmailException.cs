namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The used email exception.
    /// </summary>
    public class UsedEmailException : BaseApplicationException
    {
        /// <summary>
        /// Gets the email that is already in use.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsedEmailException" /> class.
        /// </summary>
        /// <param name="email">The email that is already in use.</param>
        public UsedEmailException(string email)
            : base("This email is already in use.")
        {
            this.Email = email;
        }
    }
}
