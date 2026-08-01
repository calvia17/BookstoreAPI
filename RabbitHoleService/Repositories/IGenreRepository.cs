using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The genre repository interface.
    /// </summary>
    public interface IGenreRepository
    {
        /// <summary>
        /// Gets all the genres.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The genres.</returns>
        Task<IEnumerable<Genre>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
