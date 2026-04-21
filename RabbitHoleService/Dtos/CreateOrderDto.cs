using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The create order dto.
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Gets the book orders.
        /// </summary>
        [Required(ErrorMessage = "The order must contain books.")]
        [MinLength(1, ErrorMessage = "The order should contain atleast one book.")]
        public required ICollection<CreateOrderItemDto> OrderItems { get; init; } = [];
    }

    /// <summary>
    /// The create book order dto.
    /// </summary>
    public class CreateOrderItemDto
    {
        /// <summary>
        /// Gets the book id.
        /// </summary>
        [Required(ErrorMessage = "The book id is required.")]
        public Guid? BookId { get; init; }

        /// <summary>
        /// Gets the quantity.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "The quantity must be greater than zero.")]
        [Required(ErrorMessage = "The quantity is required.")]
        public int? Quantity { get; init; }
    }
}