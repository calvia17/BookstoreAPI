using Azure.Core;
using Microsoft.Extensions.Caching.Hybrid;
using NuGet.Packaging.Signing;
using RabbitHoleService.Dtos;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;
using System.Threading;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The cached book repository.
    /// </summary>
    public class CachedBookRepository : ICachedBookRepository
    {
        private readonly HybridCache cache;
        private readonly IBookRepository innerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedBookRepository" /> class.
        /// </summary>
        /// <param name="bookRepository">The book repository.</param>
        /// <param name="cache">The cache.</param>
        public CachedBookRepository(IBookRepository bookRepository, HybridCache cache)
        {
            this.innerRepository = bookRepository;
            this.cache = cache;
        }

        /// <summary>
        /// Gets all the books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var books = await this.cache.GetOrCreateAsync(
                "books:all",
                async cancel => await this.innerRepository.GetAllAsync(cancel),
                tags: ["books:all"],
                cancellationToken: cancellationToken);
            return books;
        }

        /// <summary>
        /// Gets the book.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        public async Task<Book?> GetAsync(Guid id, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            var book = await this.cache.GetOrCreateAsync(
                $"book:{id}",
                async cancel => await this.innerRepository.GetAsync(id, trackChanges, cancel),
                tags: [$"book:{id}"],
                cancellationToken: cancellationToken);
            return book;
        }

        /// <summary>
        /// Gets the books by the ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<Book>> GetAsync(HashSet<Guid> ids, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            var book = await this.cache.GetOrCreateAsync(
                $"book:{string.Join(",", ids.OrderBy(id => id))}",
                async cancel => await this.innerRepository.GetAsync(ids, trackChanges, cancel),
                tags: ids.Select(id => $"book:{id}").ToList(),
                cancellationToken: cancellationToken);
            return book;
        }

        /// <summary>
        /// Gets the book by the isbn.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The book.</returns>
        public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
        {
            var book = await this.cache.GetOrCreateAsync(
                $"book:isbn:{isbn}",
                async cancel => await this.innerRepository.GetByIsbnAsync(isbn, cancel),
                tags: [$"book:isbn:{isbn}"],
                cancellationToken: cancellationToken);
            return book;
        }

        /// <summary>
        /// Gets the books by the isbns.
        /// </summary>
        /// <param name="isbns">The isbns.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The books.</returns>
        public async Task<IEnumerable<Book>> GetByIsbnsAsync(HashSet<string> isbns, CancellationToken cancellationToken = default)
        {
            var book = await this.cache.GetOrCreateAsync(
                $"book:{string.Join(",", isbns.OrderBy(id => id))}",
                async cancel => await this.innerRepository.GetByIsbnsAsync(isbns, cancel),
                tags: isbns.Select(isbn => $"book:isbn:{isbn}").ToList(),
                cancellationToken: cancellationToken);
            return book;
        }

        /// <summary>
        /// Gets the maximum last modified date of all books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The maximum last modified date.</returns>
        public async Task<DateTimeOffset> GetMaxLastModifiedAsync(CancellationToken cancellationToken = default)
        {
            var maxLastModified = await this.cache.GetOrCreateAsync(
               "books:lastModified",
               async cancel => await this.innerRepository.GetMaxLastModifiedAsync(cancel),
               tags: ["books:lastModified"],
               cancellationToken: cancellationToken);
            return maxLastModified;
        }

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="newBookData">The new book data.</param>
        public void Add(Book newBookData) => this.innerRepository.Add(newBookData);

        /// <summary>
        /// Creates multiple books.
        /// </summary>
        /// <param name="newBooksData">The new books data.</param>
        public void AddMultiple(IEnumerable<Book> newBooksData) => this.innerRepository.AddMultiple(newBooksData);

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
        public async Task<IEnumerable<Book>> FindBooksAsync(string? isbn, string? name, string? author, decimal? minimumCost, decimal? maximumCost, HashSet<GenreType>? genreIds, CancellationToken cancellationToken = default)
        {
            var books = await this.cache.GetOrCreateAsync(
                GenerateSearchKey(isbn, name, author, minimumCost, maximumCost, genreIds),
                async cancel => await this.innerRepository.FindBooksAsync(isbn, name, author, minimumCost, maximumCost, genreIds, cancel),
                tags: ["books:search"],
                cancellationToken: cancellationToken);
            return books;
        }

        /// <summary>
        /// Invalidates the cache collections for books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the invalidation operation.</returns>
        public async Task InvalidateCacheCollections(CancellationToken cancellationToken = default)
        {
            await this.cache.RemoveByTagAsync("books:all", cancellationToken);
            await this.cache.RemoveByTagAsync("books:search", cancellationToken);
            await this.cache.RemoveByTagAsync("books:lastModified", cancellationToken);
        }

        /// <summary>
        /// Invalidates the cache for specified tags.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task InvalidateCacheByTags(IEnumerable<string> tags, CancellationToken cancellationToken = default)
        {
            await this.cache.RemoveByTagAsync(tags, cancellationToken);
        }

        private static string GenerateSearchKey(string? isbn, string? name, string? author, decimal? minimumCost, decimal? maximumCost, HashSet<GenreType>? genreIds)
        {
            var keyParts = new List<string>();
            if (!string.IsNullOrEmpty(isbn))
            {
                keyParts.Add($"isbn:{isbn}");
            }
            if (!string.IsNullOrEmpty(name))
            {
                keyParts.Add($"name:{name.ToLowerInvariant()}");
            }
            if (!string.IsNullOrEmpty(author))
            {
                keyParts.Add($"author:{author.ToLowerInvariant()}");
            }
            if (minimumCost.HasValue)
            {
                keyParts.Add($"minimumCost:{minimumCost.Value}");
            }
            if (maximumCost.HasValue)
            {
                keyParts.Add($"maximumCost:{maximumCost.Value}");
            }
            if (genreIds != null && genreIds.Count > 0)
            {
                keyParts.Add($"genres:{string.Join(",", genreIds.OrderBy(g => g))}");
            }
            if (keyParts.Count == 0)
            {
                return "books:search:all";
            }

            return $"books:search:{string.Join(":", keyParts)}";
        }
    }
}
