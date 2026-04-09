using RabbitHoleService.Objects;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The create genre dto.
    /// </summary>
    public class CreateGenreDto
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        [Required(ErrorMessage = "The genre id is required.")]
        [EnumDataType(typeof(GenreType))]
        public required GenreType? Id { get; init; }
    }
}
