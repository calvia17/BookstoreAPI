using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The add order item dto.
    /// </summary>
    public class AddOrderItemDto
    {
        /// <summary>
        /// Gets the book id.
        /// </summary>
        [Required(ErrorMessage = "The book id is required.")]
        public Guid? BookId { get; init; }

        /// <summary>
        /// Gets the quantitiy.
        /// </summary>
        [Required(ErrorMessage = "The quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "The quantity must not be negative.")]
        public int? Quantity { get; init; }
    }
}
