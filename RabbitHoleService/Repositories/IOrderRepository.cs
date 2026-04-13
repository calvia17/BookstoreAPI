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
        /// <returns>The orders.</returns>
        Task<IEnumerable<Order>> GetAllAsync();

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The order.</returns>
        Task<Order?> GetAsync(Guid id, bool trackChanges = false);

        /// <summary>
        /// Gets the order by the idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <returns>The order.</returns>
        Task<Order?> GetByIdempotencyKeyAsync(Guid idempotencyKey);

        /// <summary>
        /// Gets the orders by customer id.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <returns>The orders.</returns>
        Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        void Add(Order newOrderData);
    }
}
