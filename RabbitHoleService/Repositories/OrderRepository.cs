using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        /// Begins the transaction.
        /// </summary>
        /// <returns>A task representing the transaction start.</returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await this.context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns>A task representing the transaction commit.</returns>
        public async Task CommitTransactionAsync(IDbContextTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            await transaction.CommitAsync();
        }

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns>A task representing the transaction rollback.</returns>
        public async Task RollbackTransactionAsync(IDbContextTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);
            await transaction.RollbackAsync();
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
        /// <returns>The order.</returns>
        public async Task<Order> AddAsync(Order newOrderData)
        {
            ArgumentNullException.ThrowIfNull(newOrderData);

            var createdOrder = this.context.Orders.Add(newOrderData);
            await this.context.SaveChangesAsync();
            return await this.context.Orders
                        .Include(o => o.BookOrders)
                            .ThenInclude(bo => bo.Book)
                        .FirstAsync(o => o.Id == newOrderData.Id);
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        /// <param name="updateData">The update order data.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateAsync(Order updateData)
        {
            ArgumentNullException.ThrowIfNull(updateData);
            await this.context.SaveChangesAsync();
        }
    }
}