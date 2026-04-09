namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The book not found exception.
    /// </summary>
    public class BookNotFoundException : BaseApplicationException
    {
        /// <summary>
        /// Gets the book ids that were not found.
        /// </summary>
        public IEnumerable<Guid> BookIds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookNotFoundException" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public BookNotFoundException(Guid id)
            : base($"The requested book with ID '{id}' was not found.")
        {
            this.BookIds = [id];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookNotFoundException" /> class.
        /// </summary>
        /// <param name="ids">The ids.</param>
        public BookNotFoundException(IEnumerable<Guid> ids)
            : base($"Some books were not found.")
        {
            this.BookIds = ids;
        }
    }
}
