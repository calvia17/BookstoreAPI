using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The book service.
    /// </summary>
    public class BookService : IBookService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBookCacheEvictor cacheEvictor;

        /// <summary>
        /// Initializes the book service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="cacheEvictor">The cache evictor.</param>
        public BookService(IUnitOfWork unitOfWork, IBookCacheEvictor cacheEvictor)
        {
            this.unitOfWork = unitOfWork;
            this.cacheEvictor = cacheEvictor;
        }

        /// <summary>
        /// Gets all the books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<BookDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var books =  await this.unitOfWork.CachedBooks.GetAllAsync(cancellationToken);
            var dtos = books.Select(x => BookModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        public async Task<BookDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var book = await this.unitOfWork.CachedBooks.GetAsync(id, cancellationToken: cancellationToken);
            if (book == null)
            {
                throw new BookNotFoundException(id);
            }

            return BookModelDtoMapper.ToDto(book);
        }

        /// <summary>
        /// Gets the maximum last modified date of all books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The maximum last modified date.</returns>
        public async Task<DateTimeOffset> GetMaxLastModifiedAsync(CancellationToken cancellationToken = default)
        {
            var maxLastModified = await this.unitOfWork.CachedBooks.GetMaxLastModifiedAsync(cancellationToken);
            return maxLastModified;
        }

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="newBookData">The new book data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        public async Task<BookDto> CreateAsync(CreateBookDto newBookData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(newBookData);

            var duplicateBook = await this.unitOfWork.Books.GetByIsbnAsync(newBookData.Isbn, cancellationToken);
            if (duplicateBook != null)
            {
                throw new BookAlreadyExistsException(new ExistingBook(duplicateBook.Id, duplicateBook.Isbn, duplicateBook.Name, duplicateBook.Author));
            }

            var bookToCreate = BookModelDtoMapper.ToModel(newBookData);
            this.unitOfWork.CachedBooks.Add(bookToCreate);
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            await this.cacheEvictor.InvalidateCacheCollections(cancellationToken);

            // No need to cache since it will be cached on next fetch.
            // Caching here is unnecessary overhead since it may not be fetched for a long time.
            // It would just be cache pollution and would push out frequently accessed books from cache.
            var createdBook = await this.unitOfWork.Books.GetAsync(bookToCreate.Id, cancellationToken: cancellationToken);
            return BookModelDtoMapper.ToDto(createdBook!);
        }

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added books.</returns>
        public async Task<IEnumerable<BookDto>> CreateMultipleAsync(IEnumerable<CreateBookDto> newBooksData, CancellationToken cancellationToken = default)
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

            var duplicateBooks = await this.unitOfWork.Books.GetByIsbnsAsync(isbns, cancellationToken);
            if (duplicateBooks.Any())
            {
                var conflicts = duplicateBooks.Select(db => new ExistingBook(db.Id, db.Isbn, db.Name, db.Author)).ToList();
                throw new BookAlreadyExistsException(conflicts);
            }

            var booksToCreate = newBooksData.Select(BookModelDtoMapper.ToModel).ToList();
            this.unitOfWork.CachedBooks.AddMultiple(booksToCreate);
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            await this.cacheEvictor.InvalidateCacheCollections(cancellationToken);
            var createdBookIds = booksToCreate.Select(b => b.Id).ToHashSet();
            var createdBooks = await this.unitOfWork.Books.GetAsync(createdBookIds, cancellationToken: cancellationToken);
            return createdBooks.Select(BookModelDtoMapper.ToDto).ToList();
        }

        /// <summary>
        /// Updates a book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateAsync(Guid id, UpdateBookDto updateData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var book = await this.unitOfWork.Books.GetAsync(id, true, cancellationToken);
            if (book == null)
            {
                throw new BookNotFoundException(id);
            }

            book.UpdateBookProperties(updateData.Name, updateData.Author, updateData.Cost, updateData.Stock, updateData.GenreIds);
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            await this.cacheEvictor.InvalidateCacheForBooks([book], cancellationToken);
        }

        /// <summary>
        /// Updates multiple books.
        /// </summary>
        /// <param name="updateData">The data to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateMultipleAsync(UpdateMultipleBooksDto updateData, CancellationToken cancellationToken = default)
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

            var books = await this.unitOfWork.Books.GetAsync(bookIds, true, cancellationToken);
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
                    book.UpdateBookProperties(bookUpdateData.Name, bookUpdateData.Author, bookUpdateData.Cost, bookUpdateData.Stock, bookUpdateData.GenreIds);
                }
            }

            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            await this.cacheEvictor.InvalidateCacheForBooks(books, cancellationToken);
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the delete operation.</returns>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var book = await this.unitOfWork.Books.GetAsync(id, true, cancellationToken);
            if (book == null)
            {
                throw new BookNotFoundException(id);
            }

            book.DeleteBook();
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            await this.cacheEvictor.InvalidateCacheForBooks([book], cancellationToken);

        }

        /// <summary>
        /// Finds books that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<BookDto>> FindBooksAsync(BookSearchRequestDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var books = await this.unitOfWork.CachedBooks.FindBooksAsync(request.Isbn, request.Name, request.Author, request.MinimumCost, request.MaximumCost, request.Genres, cancellationToken);
            return books.Select(BookModelDtoMapper.ToDto).ToList();
        }
    }
}