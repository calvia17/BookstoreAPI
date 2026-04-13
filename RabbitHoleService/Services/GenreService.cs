using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;

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
        /// <returns>The genres.</returns>
        public async Task<IEnumerable<GenreDto>> GetAllAsync()
        {
            var genres = await this.unitOfWork.Genres.GetAllAsync();
            var dtos = genres.Select(x => GenreModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }
    }
}
