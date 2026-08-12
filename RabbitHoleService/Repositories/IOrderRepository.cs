using Microsoft.EntityFrameworkCore.Storage;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The order repository interface.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Gets all the orders.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The order.</returns>
        Task<Order?> GetAsync(Guid id, bool trackChanges = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the order by the idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The order.</returns>
        Task<Order?> GetByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the orders by customer id.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        void Add(Order newOrderData);

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <param name="newStatus">The new status.</param>
        /// <param name="allowedPreviousStates">The allowed previous states.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> UpdateStatusAsync(Guid orderId, OrderStatus newStatus, IReadOnlyCollection<OrderStatus> allowedPreviousStates, CancellationToken cancellationToken = default);
    }
}
