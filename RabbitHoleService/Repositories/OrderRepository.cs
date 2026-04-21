using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The order repository.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly BookStoreContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderRepository" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public OrderRepository(BookStoreContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all the orders.
        /// </summary>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            var orders = await this.context.Orders
                        .AsNoTracking()
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .ToListAsync();
            return orders;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The order.</returns>
        public async Task<Order?> GetAsync(Guid id, bool trackChanges = false)
        {
            IQueryable<Order> ordersQuery = this.context.Orders;
            if (!trackChanges)
            {
                ordersQuery = ordersQuery.AsNoTracking();
            }

            var order = await ordersQuery
                        .Include(o => o.Customer)
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .FirstOrDefaultAsync(o => o.Id == id);
            return order;
        }

        /// <summary>
        /// Gets the order by the idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <returns>The order.</returns>
        public async Task<Order?> GetByIdempotencyKeyAsync(Guid idempotencyKey)
        {
            var order = await this.context.Orders
                        .AsNoTracking()
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey);
            return order;
        }

        /// <summary>
        /// Gets the orders by customer id.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
        {
            var order = await this.context.Orders
                        .AsNoTracking()
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .Where(o => o.CustomerId == customerId)
                        .ToListAsync();
            return order;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        public void Add(Order newOrderData)
        {
            ArgumentNullException.ThrowIfNull(newOrderData);
            this.context.Orders.Add(newOrderData);
        }
    }
}