using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;
using System.Text.RegularExpressions;

namespace RabbitHoleService.Mappers
{
    /// <summary>
    /// The genre model dto mapper.
    /// </summary>
    public class GenreModelDtoMapper
    {
        /// <summary>
        /// Maps a genre to its dto.
        /// </summary>
        /// <param name="genre">The egnre model.</param>
        /// <returns>The dto.</returns>
        public static GenreDto ToDto(Genre genre)
        {
            ArgumentNullException.ThrowIfNull(genre);

            return new GenreDto()
            {
                Id = genre.Id,
                Name = genre.Name
            };
        }
    }
}
