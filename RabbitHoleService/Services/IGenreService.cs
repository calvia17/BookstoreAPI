using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The genre service interface.
    /// </summary>
    public interface IGenreService
    {
        /// <summary>
        /// Gets all the genres.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The customers.</returns>
        Task<IEnumerable<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
