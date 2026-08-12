using RabbitHoleService.Dtos;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The book repository interface.
    /// </summary>
    public interface IBookRepository
    {
        /// <summary>
        /// Gets all the books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        Task<Book?> GetAsync(Guid id, bool trackChanges = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the maximum last modified date of all books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The maximum last modified date.</returns>
        Task<DateTimeOffset> GetMaxLastModifiedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the books by the ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> GetAsync(HashSet<Guid> ids, bool trackChanges = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the book by the isbn.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the books by the isbns.
        /// </summary>
        /// <param name="isbns">The isbns.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> GetByIsbnsAsync(HashSet<string> isbns, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="newBookData">The new book data.</param>
        void Add(Book newBookData);

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        void AddMultiple(IEnumerable<Book> newBooksData);

        /// <summary>
        /// Finds books that match a certain criteria.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <param name="name">The name.</param>
        /// <param name="author">The author.</param>
        /// <param name="minimumCost">The minimumCost.</param>
        /// <param name="maximumCost">The maximumCost.</param>
        /// <param name = "genreIds" > The genreIds</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> FindBooksAsync(string? isbn, string? name, string? author, decimal? minimumCost, decimal? maximumCost, HashSet<GenreType>? genreIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the book stock.
        /// </summary>
        /// <param name="bookId">The book id.</param>
        /// <param name="stockToAdd">The stock to add.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> UpdateStockAsync(Guid bookId, int stockToAdd, CancellationToken cancellationToken = default);
    }
}
