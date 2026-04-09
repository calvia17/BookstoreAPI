using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;
using RabbitHoleService.Dtos;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The book repository.
    /// </summary>
    public class BookRepository : IBookRepository
    {
        private readonly BookStoreContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookRepository" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public BookRepository(BookStoreContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all the books.
        /// </summary>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            var books = await this.context.Books
                        .AsNoTracking()
                        .Include(b => b.BookGenres)
                        .ToListAsync();
            return books;
        }

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The book.</returns>
        public async Task<Book?> GetAsync(Guid id, bool trackChanges = false)
        {
            IQueryable<Book> booksQuery = this.context.Books;
            if (!trackChanges)
            {
                booksQuery = booksQuery.AsNoTracking();
            }

            var book = await booksQuery
                        .Include(b => b.BookGenres)
                        .FirstOrDefaultAsync(b => b.Id == id);
            return book;
        }

        /// <summary>
        /// Gets the books by the ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<Book>> GetAsync(HashSet<Guid> ids, bool trackChanges = false)
        {
            IQueryable<Book> booksQuery = this.context.Books;
            if (!trackChanges)
            {
                booksQuery = booksQuery.AsNoTracking();
            }

            var books = await booksQuery
                        .Include(b => b.BookGenres)
                        .Where(b => ids.Contains(b.Id))
                        .ToListAsync();
            return books;
        }

        /// <summary>
        /// Gets the book by the isbn.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <returns>The book.</returns>
        public async Task<Book?> GetByIsbnAsync(string isbn)
        {
            var book = await this.context.Books
                        .AsNoTracking()
                        .Include(b => b.BookGenres)
                        .FirstOrDefaultAsync(b => b.Isbn == isbn);
            return book;
        }

        /// <summary>
        /// Gets the books by the isbns.
        /// </summary>
        /// <param name="isbns">The isbns.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<Book>> GetByIsbnsAsync(HashSet<string> isbns)
        {
            var books = await this.context.Books
                        .AsNoTracking()
                        .Include(b => b.BookGenres)
                        .Where(b => isbns.Contains(b.Isbn))
                        .ToListAsync();
            return books;
        }

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="newBookData">The new book.</param>
        /// <returns>The book.</returns>
        public async Task<Book> AddAsync(Book newBookData)
        {
            ArgumentNullException.ThrowIfNull(newBookData);

            var createdBook = this.context.Books.Add(newBookData);
            await this.context.SaveChangesAsync();
            return await this.context.Books
                        .Include(b => b.BookGenres)
                        .FirstAsync(b => b.Id == newBookData.Id);
        }

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        /// <returns>The added books.</returns>
        public async Task<IEnumerable<Book>> AddMultipleAsync(IEnumerable<Book> newBooksData)
        {
            ArgumentNullException.ThrowIfNull(newBooksData);

            this.context.Books.AddRange(newBooksData);
            await this.context.SaveChangesAsync();
            var createdBookIds = newBooksData.Select(b => b.Id).ToHashSet();
            return await this.context.Books
                        .Include(b => b.BookGenres)
                        .Where(b => createdBookIds.Contains(b.Id))
                        .ToListAsync();
        }

        /// <summary>
        /// Updates an existing book.
        /// </summary>
        /// <param name="updateData">The update data.</param>
        /// <returns>The task.</returns>
        public async Task UpdateAsync(Book updateData)
        {
            ArgumentNullException.ThrowIfNull(updateData);
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates multiple books.
        /// </summary>
        /// <param name="updateData">The data to update.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateMultipleAsync(IEnumerable<Book> updateData)
        {
            ArgumentNullException.ThrowIfNull(updateData);
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a book.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <returns>The task.</returns>
        public async Task DeleteAsync(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            this.context.Books.Remove(book);
            await this.context.SaveChangesAsync();
        }

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
        public async Task<IEnumerable<Book>> FindBooksAsync(string? isbn, string? name, string? author, decimal? minimumCost, decimal? maximumCost, HashSet<GenreType>? genreIds)
        {
            var books = this.context.Books.AsNoTracking().Include(b => b.BookGenres).AsQueryable();
            if (!string.IsNullOrEmpty(isbn))
            {
                books = books.Where(b => b.Isbn.StartsWith(isbn));
            }
            if (!string.IsNullOrEmpty(name))
            {
                books = books.Where(b => b.Name.StartsWith(name));
            }
            if (!string.IsNullOrEmpty(author))
            {
                books = books.Where(b => b.Author.StartsWith(author));
            }
            if (minimumCost.HasValue)
            {
                books = books.Where(b => b.Cost >= minimumCost.Value);
            }
            if (maximumCost.HasValue)
            {
                books = books.Where(b => b.Cost <= maximumCost.Value);
            }

            if (genreIds != null && genreIds.Count > 0)
            {
                books = books.Where(b => b.BookGenres.Any(bg => genreIds.Contains(bg.GenreId)));
            }

            return await books.ToListAsync();
        }
    }
}