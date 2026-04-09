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
        private readonly IGenreRepository genreRepository;
        private readonly IBookRepository bookRepository;

        /// <summary>
        /// Initializes the genre service.
        /// </summary>
        /// <param name="genreRepository">The genre repository.</param>
        /// <param name="bookRepository">The book repository.</param>
        public GenreService(IGenreRepository genreRepository, IBookRepository bookRepository)
        {
            this.genreRepository = genreRepository;
            this.bookRepository = bookRepository;
        }

        /// <summary>
        /// Gets all the genres.
        /// </summary>
        /// <returns>The genres.</returns>
        public async Task<IEnumerable<GenreDto>> GetAllAsync()
        {
            var genres = await this.genreRepository.GetAllAsync();
            var dtos = genres.Select(x => GenreModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }
    }
}
