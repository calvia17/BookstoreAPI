using RabbitHoleService.Dtos;

namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The idempotency key expired exception.
    /// </summary>
    public class IdempotencyKeyExpiredException : BaseApplicationException
    {
        /// <summary>
        /// Gets the order id.
        /// </summary>
        public Guid OrderId { get; }
        
        /// <summary>
        /// Gets the idempotency key.
        /// </summary>
        public Guid IdempotencyKey { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdempotencyKeyExpiredException" /> class.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        public IdempotencyKeyExpiredException(Guid orderId, Guid idempotencyKey)
            : base($"The request session has timed out.")
        {
            this.OrderId = orderId;
            this.IdempotencyKey = idempotencyKey;
        }
    }
}
