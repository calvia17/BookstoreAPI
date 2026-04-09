using RabbitHoleService.Objects;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The genre dto.
    /// </summary>
    public class GenreDto
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public GenreType Id { get; init; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public required string Name { get; init; }
    }
}
