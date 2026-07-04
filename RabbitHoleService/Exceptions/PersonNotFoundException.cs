namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The person not found exception.
    /// </summary>
    public class PersonNotFoundException<TEntity> : BaseApplicationException
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
        /// Initializes a new instance of the <see cref="PersonNotFoundException{TEntity}" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public PersonNotFoundException(Guid id)
            : base($"The requested {typeof(TEntity).Name} with ID '{id}' was not found.")
        {
            this.PersonId = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonNotFoundException{TEntity}" /> class.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public PersonNotFoundException(string userId)
            : base($"The requested {typeof(TEntity).Name} with user ID '{userId}' was not found.")
        {
            this.UserId = userId;
        }
    }
}
