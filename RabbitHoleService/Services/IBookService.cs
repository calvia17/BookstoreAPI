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
        /// <returns>The books.</returns>
        Task<IEnumerable<BookDto>> GetAllAsync();

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The book.</returns>
        Task<BookDto> GetAsync(Guid id);

        /// <summary>
        /// Gets the books.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<BookDto>> GetAsync(HashSet<Guid> ids);

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="newBookData">The book to create.</param>
        /// <returns>The book.</returns>
        Task<BookDto> CreateAsync(CreateBookDto newBookData);

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        /// <returns>The added books.</returns>
        Task<IEnumerable<BookDto>> CreateMultipleAsync(IEnumerable<CreateBookDto> newBooksData);

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="isAdmin">A value indicating whether the user is an admin.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateAsync(Guid id, UpdateBookDto updateData, bool isAdmin);

        /// <summary>
        /// Updates multiple books.
        /// </summary>
        /// <param name="updateData">The data to update.</param>
        /// <param name="isAdmin">A value indicating whether the user is an admin.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateMultipleAsync(UpdateMultipleBooksDto updateData, bool isAdmin);

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">The id.</param>=
        /// <returns>A task that represents the delete operation.</returns>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Finds books that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The books.</returns>
        Task<IEnumerable<BookDto>> FindBooksAsync(BookSearchRequestDto request);
    }
}
