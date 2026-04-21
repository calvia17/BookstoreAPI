using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The book service.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Initializes the book service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public BookService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets all the books.
        /// </summary>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<BookDto>> GetAllAsync()
        {
            var books =  await this.unitOfWork.Books.GetAllAsync();
            var dtos = books.Select(x => BookModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The book.</returns>
        public async Task<BookDto> GetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var book = await this.unitOfWork.Books.GetAsync(id);
            if (book == null)
            {
                throw new BookNotFoundException(id);
            }

            return BookModelDtoMapper.ToDto(book);
        }

        /// <summary>
        /// Gets the books.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<BookDto>> GetAsync(HashSet<Guid> ids)
        {
            if (ids.Any(id => id == Guid.Empty))
            {
                throw new ArgumentException("One or more invalid IDs provided.", nameof(ids));
            }

            var books = await this.unitOfWork.Books.GetAsync(ids);
            return books.Select(BookModelDtoMapper.ToDto).ToList();
        }

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="newBookData">The new book data.</param>
        /// <returns>The book.</returns>
        public async Task<BookDto> CreateAsync(CreateBookDto newBookData)
        {
            ArgumentNullException.ThrowIfNull(newBookData);

            var duplicateBook = await this.unitOfWork.Books.GetByIsbnAsync(newBookData.Isbn);
            if (duplicateBook != null)
            {
                throw new BookAlreadyExistsException(new ExistingBook(duplicateBook.Id, duplicateBook.Isbn, duplicateBook.Name, duplicateBook.Author));
            }

            var bookToCreate = BookModelDtoMapper.ToModel(newBookData);
            this.unitOfWork.Books.Add(bookToCreate);
            await this.unitOfWork.SaveChangesAsync();
            var createdBook = await this.unitOfWork.Books.GetAsync(bookToCreate.Id);
            return BookModelDtoMapper.ToDto(createdBook!);
        }

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        /// <returns>The added books.</returns>
        public async Task<IEnumerable<BookDto>> CreateMultipleAsync(IEnumerable<CreateBookDto> newBooksData)
        {
            ArgumentNullException.ThrowIfNull(newBooksData);

            if (!newBooksData.Any())
            {
                return Enumerable.Empty<BookDto>();
            }

            var isbns = new HashSet<string>();
            var duplicates = new HashSet<string>();
            foreach (var newBookData in newBooksData)
            {
                if (!isbns.Add(newBookData.Isbn))
                {
                    duplicates.Add(newBookData.Isbn);
                }
            }
            if (duplicates.Count > 0)
            {
                throw new DuplicateBookInputException(duplicates);
            }

            var duplicateBooks = await this.unitOfWork.Books.GetByIsbnsAsync(isbns);
            if (duplicateBooks.Any())
            {
                var conflicts = duplicateBooks.Select(db => new ExistingBook(db.Id, db.Isbn, db.Name, db.Author)).ToList();
                throw new BookAlreadyExistsException(conflicts);
            }

            var booksToCreate = newBooksData.Select(BookModelDtoMapper.ToModel).ToList();
            this.unitOfWork.Books.AddMultiple(booksToCreate);
            await this.unitOfWork.SaveChangesAsync();
            var createdBookIds = booksToCreate.Select(b => b.Id).ToHashSet();
            var createdBooks = await this.unitOfWork.Books.GetAsync(createdBookIds);
            return createdBooks.Select(BookModelDtoMapper.ToDto).ToList();
        }

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="isAdmin">A value indicating whether the user is an admin.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateAsync(Guid id, UpdateBookDto updateData, bool isAdmin)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var book = await this.unitOfWork.Books.GetAsync(id, true);
            if (book == null)
            {
                throw new BookNotFoundException(id);
            }

            UpdateBookProperties(updateData, book, isAdmin);
            await this.unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Updates multiple books.
        /// </summary>
        /// <param name="updateData">The data to update.</param>
        /// <param name="isAdmin">A value indicating whether the user is an admin.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateMultipleAsync(UpdateMultipleBooksDto updateData, bool isAdmin)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            if (updateData.Books.Any(x => x.Id == Guid.Empty))
            {
                throw new ArgumentException("One or more books have an invalid ID.", nameof(updateData.Books));
            }

            var bookIds = new HashSet<Guid>();
            var duplicateBookIds = new HashSet<Guid>();
            foreach (var bookData in updateData.Books)
            {
                if (!bookIds.Add(bookData.Id))
                {
                    duplicateBookIds.Add(bookData.Id);
                }
            }
            if (duplicateBookIds.Count > 0)
            {
                throw new DuplicateBookInputException(duplicateBookIds);
            }

            var books = await this.unitOfWork.Books.GetAsync(bookIds, true);
            var notFoundBookIds = bookIds.Except(books.Select(b => b.Id)).ToList();
            if (notFoundBookIds.Count > 0)
            {
                throw new BookNotFoundException(notFoundBookIds);
            }

            var updateDataMap = updateData.Books.ToDictionary(b => b.Id, b => b.UpdateData);
            foreach (var book in books)
            {
                if (updateDataMap.TryGetValue(book.Id, out var bookUpdateData))
                {
                    UpdateBookProperties(bookUpdateData, book, isAdmin);
                }
            }

            await this.unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">The id.</param>=
        /// <returns>A task that represents the delete operation.</returns>
        public async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var book = await this.unitOfWork.Books.GetAsync(id, true);
            if (book == null)
            {
                throw new BookNotFoundException(id);
            }

            book.IsDeleted = true;
            await this.unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Finds books that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<BookDto>> FindBooksAsync(BookSearchRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            var books = await this.unitOfWork.Books.FindBooksAsync(request.Isbn, request.Name, request.Author, request.MinimumCost, request.MaximumCost, request.Genres);
            return books.Select(BookModelDtoMapper.ToDto).ToList();
        }

        private static void UpdateBookProperties(UpdateBookDto updateData, Book book, bool isAdmin)
        {
            if (!string.IsNullOrEmpty(updateData.Name))
            {
                book.Name = updateData.Name;
            }

            if (!string.IsNullOrEmpty(updateData.Author))
            {
                book.Author = updateData.Author;
            }

            if (updateData.Cost.HasValue)
            {
                if (isAdmin)
                {
                    book.Cost = updateData.Cost.Value;
                }
                else
                {
                    throw new UnauthorizedAccessException("Only admins can update the cost of a book.");
                }
            }

            if (updateData.Stock.HasValue)
            {
                book.Stock = updateData.Stock.Value;
            }

            if (updateData.GenreIds != null)
            {
                BookService.SyncGenres(book, updateData.GenreIds);
            }
        }

        private static void SyncGenres(Book book, HashSet<GenreType> newGenreIds)
        {
            var genresToRemove = book.BookGenres.Where(bg => !newGenreIds.Contains(bg.GenreId)).ToList();
            var existingGenreIds = book.BookGenres.Select(bg => bg.GenreId).ToHashSet();
            var genresToAdd = newGenreIds.Where(g => !existingGenreIds.Contains(g)).Select(g => new BookGenre(book.Id, g)).ToList();
            genresToRemove.ForEach(bg => book.BookGenres.Remove(bg));
            genresToAdd.ForEach(bg => book.BookGenres.Add(bg));
        }
    }
}