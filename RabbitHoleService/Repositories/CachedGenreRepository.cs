using Microsoft.Extensions.Caching.Hybrid;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The cached genre repository.
    /// </summary>
    public class CachedGenreRepository : IGenreRepository
    {
        private readonly HybridCache cache;
        private readonly IGenreRepository innerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedGenreRepository" /> class.
        /// </summary>
        /// <param name="genreRepository">The genre repository.</param>
        /// <param name="cache">The cache.</param>
        public CachedGenreRepository(IGenreRepository genreRepository, HybridCache cache)
        {
            this.innerRepository = genreRepository;
            this.cache = cache;
        }

        /// <summary>
        /// Gets all the genres.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The genres.</returns>
        public async Task<IEnumerable<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var genres = await this.cache.GetOrCreateAsync(
                "genres:all",
                async cancel => await this.innerRepository.GetAllAsync(cancel),
                options: new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromDays(30),
                    LocalCacheExpiration = TimeSpan.FromDays(7)
                },
                cancellationToken: cancellationToken);
            return genres;
        }
    }
}
