using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// Represents the joining entity for the many-to-many relationship between <see cref="Book"/> and <see cref="Order"/> entities.
    /// </summary>
    public class BookOrder
    {
        /// <summary>
        /// Gets the book id.
        /// </summary>
        [Required]
        public Guid BookId { get; init; }

        /// <summary>
        /// Gets the order id.
        /// </summary>
        [Required]
        public Guid OrderId { get; init; }

        /// <summary>
        /// Gets or sets the book.
        /// </summary>
        public Book Book { get; set; } = null!;

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public Order Order { get; set; } = null!;

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price at purchase.
        /// </summary>
        [Required]
        public decimal PriceAtPurchase { get; set; }

        /// <summary>
        /// Initializes and instance of BookOrder.
        /// </summary>
        /// <param name="bookId">The book id.</param>
        /// <param name="orderId">The order id.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="priceAtPurchase">The price at purchase.</param>
        public BookOrder(Guid bookId, Guid orderId, int quantity, decimal priceAtPurchase)
        {
            this.BookId = bookId;
            this.OrderId = orderId;
            this.Quantity = quantity;
            this.PriceAtPurchase = priceAtPurchase;
        }
    }
}
