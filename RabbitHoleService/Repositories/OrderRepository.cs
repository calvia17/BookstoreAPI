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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var orders = await this.context.Orders
                        .AsNoTracking()
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .ToListAsync(cancellationToken);
            return orders;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The order.</returns>
        public async Task<Order?> GetAsync(Guid id, bool trackChanges = false, CancellationToken cancellationToken = default)
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
                        .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
            return order;
        }

        /// <summary>
        /// Gets the order by the idempotency key.
        /// </summary>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The order.</returns>
        public async Task<Order?> GetByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
        {
            var order = await this.context.Orders
                        .AsNoTracking()
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, cancellationToken);
            return order;
        }

        /// <summary>
        /// Gets the orders by customer id.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            var order = await this.context.Orders
                        .AsNoTracking()
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .Where(o => o.CustomerId == customerId)
                        .ToListAsync(cancellationToken);
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

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <param name="newStatus">The new status.</param>
        /// <param name="allowedPreviousStates">The allowed previous states.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        public async Task<int> UpdateStatusAsync(Guid orderId, OrderStatus newStatus, IReadOnlyCollection<OrderStatus> allowedPreviousStates, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(orderId);
            var affectedRows = await this.context.Orders.Where(o => o.Id == orderId && allowedPreviousStates.Contains(o.Status))
                .ExecuteUpdateAsync(x => x.SetProperty(o => o.Status, newStatus), cancellationToken);
            return affectedRows;
        }
    }
}