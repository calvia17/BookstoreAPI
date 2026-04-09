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
        /// <param name="newBookData">The new book data.</param>
        /// <returns>The book.</returns>
        Task<Book> AddAsync(Book newBookData);

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        /// <returns>The added books.</returns>
        Task<IEnumerable<Book>> AddMultipleAsync(IEnumerable<Book> newBooksData);

        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <param name="updateData">The update book data.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateAsync(Book updateData);

        /// <summary>
        /// Updates multiple books.
        /// </summary>
        /// <param name="updateData">The data to update.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateMultipleAsync(IEnumerable<Book> updateData);

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <returns>A task that represents the delete operation.</returns>
        Task DeleteAsync(Book book);

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
