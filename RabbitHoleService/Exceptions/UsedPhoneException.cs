namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The used phone exception.
    /// </summary>
    public class UsedPhoneException : BaseApplicationException
    {
        /// <summary>
        /// Gets the phone number that is already in use.
        /// </summary>
        public string Phone { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsedPhoneException" /> class.
        /// </summary>
        /// <param name="phone">The phone.</param>
        public UsedPhoneException(string phone)
            : base("This phone number is already in use.")
        {
            this.Phone = phone;
        }
    }
}
