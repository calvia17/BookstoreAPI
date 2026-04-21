using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The order class.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// The id.
        /// </summary>
        [Key]
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the idempotency key.
        /// </summary>
        public Guid IdempotencyKey { get; init; }

        /// <summary>
        /// Gets the point in time when the order was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>
        /// The order status.
        /// </summary>
        [Required]
        public OrderStatus Status { get; private set; }

        /// <summary>
        /// The customer id.
        /// </summary>
        [Required]
        public Guid CustomerId { get; init; }

        /// <summary>
        /// The customer.
        /// </summary>
        public Customer Customer { get; set; } = null!;

        /// <summary>
        /// The total cost.
        /// </summary>
        [Required]
        public decimal TotalCost { get; private set; }

        /// <summary>
        /// Gets the book orders.
        /// </summary>
        public ICollection<BookOrder> BookOrders { get; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="Order" /> class.
        /// </summary>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="customerId">The customer id.</param>
        public Order(Guid idempotencyKey, Guid customerId)
        {
            this.IdempotencyKey = idempotencyKey;
            this.CreatedAt = DateTimeOffset.UtcNow;
            this.CustomerId = customerId;
            this.Status = OrderStatus.Pending;
            this.BookOrders = new List<BookOrder>();
        }

        /// <summary>
        /// Adds a book to the order.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="quantity">The quantity.</param>
        public void AddBook(Book book, int quantity)
        {
            var bookOrder = new BookOrder(book.Id, this.Id, quantity, book.Cost);
            this.BookOrders.Add(bookOrder);
            this.TotalCost += bookOrder.PriceAtPurchase * bookOrder.Quantity;
        }

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="newStatus">The new status</param>
        /// <param name="validateTransition">A value indciating whether the state transition must be validated.</param>
        /// <returns>A value indicating whether the status update was successful.</returns>
        public bool UpdateStatus(OrderStatus newStatus, bool validateTransition = true)
        {
            if (this.Status == newStatus || (validateTransition && !this.IsStatusChangeValid(newStatus)))
            {
                return false;
            }

            this.Status = newStatus;
            return true;
        }

        private bool IsStatusChangeValid(OrderStatus newStatus)
        {
            return this.Status switch
            {
                OrderStatus.Pending => newStatus == OrderStatus.Cancelled,
                OrderStatus.Processed => newStatus == OrderStatus.Shipped,
                OrderStatus.Shipped => newStatus == OrderStatus.Delivered,
                _ => false
            };
        }
    }
}
