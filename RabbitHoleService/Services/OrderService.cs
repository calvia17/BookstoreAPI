using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The order service.
    /// </summary>
    public partial class OrderService : IOrderService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IBookCacheEvictor cacheEvictor;

        /// <summary>
        /// Initializes the order service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="cacheEvictor">The cache evictor.</param>
        public OrderService(IUnitOfWork unitOfWork, IBookCacheEvictor cacheEvictor)
        {
            this.unitOfWork = unitOfWork;
            this.cacheEvictor = cacheEvictor;
        }

        /// <summary>
        /// Gets all the orders.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var orders = await this.unitOfWork.Orders.GetAllAsync(cancellationToken);
            var dtos = orders.Select(x => OrderModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="isStaffOrAdmin">Indicates if the user is staff or admin.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> GetAsync(Guid id, bool isStaffOrAdmin, string userId, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid order ID provided.", nameof(id));
            }

            var order = await this.unitOfWork.Orders.GetAsync(id, cancellationToken: cancellationToken);
            if (order == null || (!isStaffOrAdmin && order.Customer.UserId != userId))
            {
                throw new OrderNotFoundException(id);
            }

            return OrderModelDtoMapper.ToDto(order);
        }

        /// <summary>
        /// Gets the orders for a customer.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetOrdersForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(customerId));
            }

            var orders = await this.unitOfWork.Orders.GetByCustomerIdAsync(customerId, cancellationToken);
            return orders.Select(x => OrderModelDtoMapper.ToDto(x));
        }

        /// <summary>
        /// Gets the orders for the authenticated customer.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string userId, CancellationToken cancellationToken = default)
        {
            var customer = await this.unitOfWork.Customers.GetByUserIdAsync(userId, cancellationToken: cancellationToken);
            if (customer == null)
            {
                throw new PersonNotFoundException<Customer>(userId);
            }

            var orders = await this.unitOfWork.Orders.GetByCustomerIdAsync(customer.Id, cancellationToken);
            return orders.Select(x => OrderModelDtoMapper.ToDto(x));
        }

        /// <summary>
        /// Creates a new order for a customer.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> CreateForCustomerAsync(string idempotencyKey, CreateOrderForCustomerDto newOrderData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(newOrderData);
            ArgumentException.ThrowIfNullOrEmpty(idempotencyKey);

            var customer = await this.unitOfWork.Customers.GetAsync(newOrderData.CustomerId!.Value, cancellationToken: cancellationToken);
            if (customer == null)
            {
                throw new PersonNotFoundException<Customer>(newOrderData.CustomerId!.Value);
            }

            return await CreateAsync(idempotencyKey, newOrderData, customer.Id, cancellationToken);
        }

        /// <summary>
        /// Creates a new order for the authenticated customer.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="userId">The userId.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> CreateAsync(string idempotencyKey, CreateOrderDto newOrderData, string userId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(newOrderData);
            ArgumentException.ThrowIfNullOrEmpty(idempotencyKey);

            var customer = await this.unitOfWork.Customers.GetByUserIdAsync(userId, cancellationToken: cancellationToken);
            if (customer == null)
            {
                throw new PersonNotFoundException<Customer>(userId);
            }

            return await CreateAsync(idempotencyKey, newOrderData, customer.Id, cancellationToken);
        }

        private async Task<OrderDto> CreateAsync(string idempotencyKey, CreateOrderDto newOrderData, Guid customerId, CancellationToken cancellationToken)
        {
            var (existingOrder, idempotencyKeyValue) = await this.ValidateIdempotencyKey(idempotencyKey, cancellationToken);
            if (existingOrder != null)
            {
                return existingOrder;
            }

            var books = await this.ValidateBooks(newOrderData, cancellationToken);
            var booksMap = books.ToDictionary(x => x.Id);

            return await ProcessTransaction(idempotencyKeyValue, newOrderData, booksMap, customerId, cancellationToken);
        }

        private async Task<OrderDto> ProcessTransaction(Guid idempotencyKey, CreateOrderDto newOrderData, Dictionary<Guid, Book> booksMap, Guid customerId, CancellationToken cancellationToken)
        {
            await using (var transaction = await this.unitOfWork.BeginTransactionAsync(cancellationToken))
            {
                // Create the order with Pending status.
                var orderToCreate = new Order(idempotencyKey, customerId);
                this.unitOfWork.Orders.Add(orderToCreate);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                var createdOrder = await this.unitOfWork.Orders.GetAsync(orderToCreate.Id, true, cancellationToken);

                if (createdOrder == null)
                {
                    throw new InvalidOperationException("Order was not found after creation");
                }

                // Add books to the order and update stock
                foreach (var item in newOrderData.OrderItems)
                {
                    if (booksMap.TryGetValue(item.BookId!.Value, out var book))
                    {
                        createdOrder.AddBook(book, item.Quantity!.Value);
                        var affectedRows = await this.unitOfWork.Books.UpdateStockAsync(book.Id, -item.Quantity!.Value, cancellationToken);
                        if (affectedRows == 0)
                        {
                            throw new InsufficientStockException(book.Id, book.Isbn, book.Name);
                        }
                    }
                }

                // Update the order status to Processed and save all the changes.
                var validPreviousStates = this.GetValidPreviousOrderStates(OrderStatus.Processed, false);
                await this.unitOfWork.Orders.UpdateStatusAsync(createdOrder.Id, OrderStatus.Processed, validPreviousStates, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);
                await this.cacheEvictor.InvalidateCacheForBooks(booksMap.Values, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return OrderModelDtoMapper.ToDto(createdOrder);
            }
        }

        private async Task<IEnumerable<Book>> ValidateBooks(CreateOrderDto newOrderData, CancellationToken cancellationToken)
        {
            // Check for duplicate books in the order
            var bookIds = new HashSet<Guid>();
            var duplicateBookIds = new HashSet<Guid>();
            foreach (var item in newOrderData.OrderItems)
            {
                if (!bookIds.Add(item.BookId!.Value))
                {
                    duplicateBookIds.Add(item.BookId!.Value);
                }
            }

            if (duplicateBookIds.Count > 0)
            {
                var duplicateBooks = await this.unitOfWork.Books.GetAsync(duplicateBookIds, cancellationToken: cancellationToken);
                var duplicateItems = duplicateBooks.Select(book => new DuplicateItem(book.Id, book.Isbn, book.Name));
                throw new DuplicateItemException(duplicateItems);
            }

            // Check if all the books exist
            var books = await this.unitOfWork.Books.GetAsync(bookIds, cancellationToken: cancellationToken);
            var notFoundBookIds = bookIds.Except(books.Select(x => x.Id)).ToList();
            if (notFoundBookIds.Count > 0)
            {
                throw new BookNotFoundException(notFoundBookIds);
            }

            return books;
        }

        private async Task<(OrderDto? ExistingOrder, Guid IdempotencyKey)> ValidateIdempotencyKey(string idempotencyKey, CancellationToken cancellationToken)
        {
            if (idempotencyKey.Length > 50 
                || !Guid.TryParse(idempotencyKey, out var idempotencyKeyValue) ||
                idempotencyKeyValue == Guid.Empty)
            {
                throw new ArgumentException("Invalid idempotency key provided.", nameof(idempotencyKey));
            }

            var duplicateOrder = await this.unitOfWork.Orders.GetByIdempotencyKeyAsync(idempotencyKeyValue, cancellationToken);
            if (duplicateOrder != null)
            {
                if (duplicateOrder.Status == OrderStatus.Pending)
                {
                    throw new OrderProcessingException(duplicateOrder.Id);
                }
                else
                {
                    return (OrderModelDtoMapper.ToDto(duplicateOrder), idempotencyKeyValue);
                }
            }

            return (null, idempotencyKeyValue);
        }

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="id">The order id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <param name="isAdmin">Indicates if the user is an admin.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateStatusAsync(Guid id, UpdateOrderStatusDto updateData, bool isAdmin, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            await using (var transaction = await this.unitOfWork.BeginTransactionAsync(cancellationToken))
            {
                var booksToUpdateMap = new Dictionary<Guid, Book>();
                var validPreviousStates = this.GetValidPreviousOrderStates(updateData.Status!.Value, isAdmin);
                var affectedRows = await this.unitOfWork.Orders.UpdateStatusAsync(id, updateData.Status!.Value, validPreviousStates, cancellationToken);
                if (affectedRows == 0)
                {
                    var order = await this.unitOfWork.Orders.GetAsync(id, cancellationToken: cancellationToken);
                    if (order == null)
                    {
                        throw new OrderNotFoundException(id);
                    }

                    if (order.Status == updateData.Status!.Value)
                    {
                        return;
                    }

                    throw new InvalidOrderStatusChangeException(order.Id, order.Status, updateData.Status!.Value);
                }

                if (updateData.Status!.Value == OrderStatus.Cancelled)
                {
                    // Update stock for books in the cancelled order
                    var order = await this.unitOfWork.Orders.GetAsync(id, cancellationToken: cancellationToken);
                    var bookIdsToUpdate = order!.BookOrders.Select(x => x.BookId).ToHashSet();
                    if (bookIdsToUpdate.Count > 0)
                    {
                        var booksToUpdate = await this.unitOfWork.Books.GetAsync(bookIdsToUpdate, cancellationToken: cancellationToken);
                        booksToUpdateMap = booksToUpdate.ToDictionary(x => x.Id);
                        foreach (var item in order.BookOrders)
                        {
                            if (booksToUpdateMap.TryGetValue(item.BookId, out var book))
                            {
                                await this.unitOfWork.Books.UpdateStockAsync(book.Id, item.Quantity, cancellationToken);
                            }
                        }
                    }
                }

                if (updateData.Status!.Value == OrderStatus.Cancelled && booksToUpdateMap.Count > 0)
                {
                    await this.cacheEvictor.InvalidateCacheForBooks(booksToUpdateMap.Values, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Gets the valid previous order states for a given new status.
        /// </summary>
        /// <param name="newStatus">The new status</param>
        /// <param name="validateTransition">A value indicating whether validation should be performed.</param>
        /// <returns>The valid previous states.</returns>
        public IReadOnlyCollection<OrderStatus> GetValidPreviousOrderStates(OrderStatus newStatus, bool validateTransition)
        {
            var previousStates = Enum.GetValues<OrderStatus>()
                .Where(status => IsStatusChangeValid(status, newStatus, validateTransition))
                .ToList();
            return previousStates;
        }

        private static bool IsStatusChangeValid(OrderStatus previousStatus, OrderStatus newStatus, bool validateTransition)
        {
            if (previousStatus == newStatus)
            {
                return false;
            }
            
            if (validateTransition)
            {
                return true;
            }

            return previousStatus switch
            {
                OrderStatus.Pending => newStatus == OrderStatus.Processed || newStatus == OrderStatus.Cancelled,
                OrderStatus.Processed => newStatus == OrderStatus.Shipped,
                OrderStatus.Shipped => newStatus == OrderStatus.Delivered,
                _ => false
            };
        }
    }
}