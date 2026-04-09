using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The genre repository.
    /// </summary>
    public class GenreRepository : IGenreRepository
    {
        private readonly BookStoreContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenreRepository" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public GenreRepository(BookStoreContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all the genres.
        /// </summary>
        /// <returns>The genres.</returns>
        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            var genres = await this.context.Genres.AsNoTracking().ToListAsync();
            return genres;
        }
    }
}
