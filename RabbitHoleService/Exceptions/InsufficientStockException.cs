namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The insufficient stock exception.
    /// </summary>
    public class InsufficientStockException : BaseApplicationException
    {
        /// <summary>
        /// Gets the book id.
        /// </summary>
        public Guid BookId { get; }

        /// <summary>
        /// Gets the book isbn.
        /// </summary>
        public string Isbn { get; }

        /// <summary>
        /// Gets the book name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientStockException" /> class.
        /// </summary>
        /// <param name="id">The book id.</param>
        /// <param name="isbn">The book isbn.</param>
        /// <param name="name">The book name.</param>
        public InsufficientStockException(Guid id, string isbn, string name)
            : base($"Insufficient stock to complete the order.")
        {
            this.BookId = id;
            this.Isbn = isbn;
            this.Name = name;
        }
    }
}
