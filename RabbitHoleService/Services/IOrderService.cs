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
        /// <param name="isStaffOrAdmin">Indicates if the user is staff or admin.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>The order.</returns>
        Task<OrderDto> GetAsync(Guid id, bool isStaffOrAdmin, string userId);

        /// <summary>
        /// Creates a new order for a customer.
        /// </summary>
        /// <param name="newOrderData">The order to create.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <returns>The order.</returns>
        Task<OrderDto> CreateForCustomerAsync(string idempotencyKey, CreateOrderForCustomerDto newOrderData);

        /// <summary>
        /// Creates a new order for the authenticated customer.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="userId">The userId.</param>
        /// <returns>The order.</returns>
        Task<OrderDto> CreateAsync(string idempotencyKey, CreateOrderDto newOrderData, string userId);

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="id">The order id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <param name="isAdmin">Indicates if the user is an admin.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateStatusAsync(Guid id, UpdateOrderStatusDto updateData, bool isAdmin);

        /// <summary>
        /// Gets the orders for a customer.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <returns>The orders.</returns>
        Task<IEnumerable<OrderDto>> GetOrdersForCustomerAsync(Guid customerId);

        /// <summary>
        /// Gets the orders for the authenticated customer.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The orders.</returns>
        Task<IEnumerable<OrderDto>> GetOrdersAsync(string userId);
    }
}
