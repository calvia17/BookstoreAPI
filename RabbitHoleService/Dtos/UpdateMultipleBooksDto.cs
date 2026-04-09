using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The book record.
    /// </summary>
    /// <param name="Id">The id.</param>
    /// <param name="UpdateData">The update data.</param>
    public record BookData (Guid Id, UpdateBookDto UpdateData);

    /// <summary>
    /// The update mutliple books dto.
    /// </summary>
    public class UpdateMultipleBooksDto
    {
        /// <summary>
        /// Gets the books to update.
        /// </summary>
        [Required(ErrorMessage = "The book list is required.")]
        [MinLength(1, ErrorMessage = "The book list should contain at least one book.")]
        public required IEnumerable<BookData> Books { get; init; }
    }
}
