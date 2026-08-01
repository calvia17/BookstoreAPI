using RabbitHoleService.Dtos;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The book service interface.
    /// </summary>
    public interface IBookService
    {
        /// <summary>
        /// Gets all the books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<BookDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        Task<BookDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the maximum last modified date of all books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The maximum last modified date.</returns>
        Task<DateTimeOffset> GetMaxLastModifiedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="newBookData">The book to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        Task<BookDto> CreateAsync(CreateBookDto newBookData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added books.</returns>
        Task<IEnumerable<BookDto>> CreateMultipleAsync(IEnumerable<CreateBookDto> newBooksData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateAsync(Guid id, UpdateBookDto updateData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates multiple books.
        /// </summary>
        /// <param name="updateData">The data to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateMultipleAsync(UpdateMultipleBooksDto updateData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the delete operation.</returns>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds books that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<BookDto>> FindBooksAsync(BookSearchRequestDto request, CancellationToken cancellationToken = default);
    }
}
