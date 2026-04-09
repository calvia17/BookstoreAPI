using RabbitHoleService.Objects;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The order dto.
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the point in time when the order was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// Gets the order status.
        /// </summary>
        public required string Status { get; init; }

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        public decimal TotalCost { get; init; }

        /// <summary>
        /// Gets the customer id.
        /// </summary>
        public Guid CustomerId { get; init; }

        /// <summary>
        /// Gets the order items.
        /// </summary>
        public required ICollection<OrderItemDto> OrderItems { get; init; }
    }

    /// <summary>
    /// The order item dto.
    /// </summary>
    public class OrderItemDto
    {
        /// <summary>
        /// Gets the book id.
        /// </summary>
        public Guid BookId { get; init; }

        /// <summary>
        /// Get the book ISBN.
        /// </summary>
        public required string Isbn { get; init; }

        /// <summary>
        /// Get the book name.
        /// </summary>
        public required string BookName { get; init; }

        /// <summary>
        /// Gets the quantity.
        /// </summary>
        public int Quantity { get; init; }

        /// <summary>
        /// Gets the price at purchase.
        /// </summary>
        public decimal PriceAtPurchase { get; init; }
    }
}
