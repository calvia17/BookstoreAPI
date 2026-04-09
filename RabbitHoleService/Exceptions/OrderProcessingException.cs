using RabbitHoleService.Dtos;

namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The order processing exception.
    /// </summary>
    public class OrderProcessingException : BaseApplicationException
    {
        /// <summary>
        /// Gets the order id.
        /// </summary>
        public Guid OrderId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderProcessingException" /> class.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        public OrderProcessingException(Guid orderId)
            : base($"The order is being processed.")
        {
            this.OrderId = orderId;
        }
    }
}
