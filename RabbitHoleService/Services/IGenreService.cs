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
        /// <returns>The customers.</returns>
        Task<IEnumerable<GenreDto>> GetAllAsync();
    }
}
