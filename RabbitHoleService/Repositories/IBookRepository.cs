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
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> GetAllAsync();

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The book.</returns>
        Task<Book?> GetAsync(Guid id, bool trackChanges = false);

        /// <summary>
        /// Gets the books by the ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> GetAsync(HashSet<Guid> ids, bool trackChanges = false);

        /// <summary>
        /// Gets the book by the isbn.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <returns>The book.</returns>
        Task<Book?> GetByIsbnAsync(string isbn);

        /// <summary>
        /// Gets the books by the isbns.
        /// </summary>
        /// <param name="isbns">The isbns.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> GetByIsbnsAsync(HashSet<string> isbns);

        /// <summary>
        /// Creates a new book.
        /// </summary>
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
        /// <returns>The books.</returns>
        Task<IEnumerable<Book>> FindBooksAsync(string? isbn, string? name, string? author, decimal? minimumCost, decimal? maximumCost, HashSet<GenreType>? genreIds);
    }
}
