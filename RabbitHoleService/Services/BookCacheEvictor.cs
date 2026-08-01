using Microsoft.AspNetCore.OutputCaching;
using RabbitHoleService.Data;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The cache evictor class.
    /// </summary>
    public class BookCacheEvictor : IBookCacheEvictor
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IOutputCacheStore cacheStore;

        /// <summary>
        /// Initializes the book service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="cacheStore">The cache store.</param>
        public BookCacheEvictor(IUnitOfWork unitOfWork, IOutputCacheStore cacheStore)
        {
            this.unitOfWork = unitOfWork;
            this.cacheStore = cacheStore;
        }

        /// <summary>
        /// Invalidates the cache collections for books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the invalidation operation.</returns>
        public async Task InvalidateCacheCollections(CancellationToken cancellationToken = default)
        {
            await this.unitOfWork.CachedBooks.InvalidateCacheCollections(cancellationToken);
            await this.cacheStore.EvictByTagAsync("books:all", cancellationToken);
            await this.cacheStore.EvictByTagAsync("books:search", cancellationToken);
        }

        /// <summary>
        /// Invalidates the cache for specific books.
        /// </summary>
        /// <param name="books">The book ids.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task InvalidateCacheForBooks(IEnumerable<Book> books, CancellationToken cancellationToken = default)
        {
            await this.InvalidateCacheCollections(cancellationToken);
            var bookIdTags = books.Select(book => $"book:{book.Id}");
            var bookIsbnTags = books.Select(book => $"book:isbn:{book.Isbn}");
            List<string> allTags = [.. bookIdTags, .. bookIsbnTags];
            await this.unitOfWork.CachedBooks.InvalidateCacheByTags(allTags, cancellationToken);

            foreach (var tag in bookIdTags)
            {
                await this.cacheStore.EvictByTagAsync(tag, cancellationToken);
            }
        }
    }
}
