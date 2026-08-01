using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The book cache evictor interface.
    /// </summary>
    public interface IBookCacheEvictor
    {
        /// <summary>
        /// Invalidates the cache collections for books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the invalidation operation.</returns>
        Task InvalidateCacheCollections(CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates the cache for specific books.
        /// </summary>
        /// <param name="books">The book ids.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task InvalidateCacheForBooks(IEnumerable<Book> books, CancellationToken cancellationToken = default);
    }
}
