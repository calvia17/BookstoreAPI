using RabbitHoleService.Objects;

namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The invalid order status change exception.
    /// </summary>
    public class InvalidOrderStatusChangeException : BaseApplicationException
    {
        /// <summary>
        /// Gets the order id.
        /// </summary>
        public Guid OrderId { get; }

        /// <summary>
        /// The old status.
        /// </summary>
        public OrderStatus OldStatus { get; }

        /// <summary>
        /// The new status.
        /// </summary>
        public OrderStatus NewStatus { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOrderStatusChangeException" /> class.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <param name="oldStatus">The old status.</param>
        /// <param name="newStatus">The new status.</param>
        public InvalidOrderStatusChangeException(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus)
            : base($"The order status cannot be changed from {oldStatus} to {newStatus}.")
        {
            this.OrderId = orderId;
            this.OldStatus = oldStatus;
            this.NewStatus = newStatus;
        }
    }
}
