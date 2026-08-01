using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The cached book repository interface.
    /// </summary>
    public interface ICachedBookRepository : IBookRepository
    {
        /// <summary>
        /// Invalidates the cache collections for books.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the invalidation operation.</returns>
        Task InvalidateCacheCollections(CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidates the cache for specified tags.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task InvalidateCacheByTags(IEnumerable<string> tags, CancellationToken cancellationToken = default);
    }
}
