using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.Dtos;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The order service interface.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Gets all the orders.
        /// </summary>
        /// <returns>The orders.</returns>
        Task<IEnumerable<OrderDto>> GetAllAsync();

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The order.</returns>
        Task<OrderDto> GetAsync(Guid id);

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="newOrderData">The order to create.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <returns>The order.</returns>
        Task<OrderDto> CreateAsync(string idempotencyKey, CreateOrderDto newOrderData);

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="id">The order id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateStatusAsync(Guid id, UpdateOrderStatusDto updateData);

        /// <summary>
        /// Gets the orders for a customer.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <returns>The orders.</returns>
        Task<IEnumerable<OrderDto>> GetOrdersForCustomerAsync(Guid customerId);
    }
}
