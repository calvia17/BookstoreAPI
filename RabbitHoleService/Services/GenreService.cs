using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Mappers;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The genre service.
    /// </summary>
    public class GenreService : IGenreService
    {
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Initializes the genre service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public GenreService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets all the genres.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The genres.</returns>
        public async Task<IEnumerable<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var genres = await this.unitOfWork.Genres.GetAllAsync(cancellationToken);
            var dtos = genres.Select(x => GenreModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }
    }
}

// If a user closes the session, the cancellation token is used to cancel the lookup.
// If it is checking the cache, the final argument passed into getorcreateasync is used.
// If it is checking the database, the cancellation token is passed into the repository method to cancel the lookup.