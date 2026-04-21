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

        /// <summary>
        /// Initializes the order service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public OrderService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets all the orders.
        /// </summary>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await this.unitOfWork.Orders.GetAllAsync();
            var dtos = orders.Select(x => OrderModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="isStaffOrAdmin">Indicates if the user is staff or admin.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> GetAsync(Guid id, bool isStaffOrAdmin, string userId)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid order ID provided.", nameof(id));
            }

            var order = await this.unitOfWork.Orders.GetAsync(id);
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
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetOrdersForCustomerAsync(Guid customerId)
        {
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(customerId));
            }

            var orders = await this.unitOfWork.Orders.GetByCustomerIdAsync(customerId);
            return orders.Select(x => OrderModelDtoMapper.ToDto(x));
        }

        /// <summary>
        /// Gets the orders for the authenticated customer.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(string userId)
        {
            var customer = await this.unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
            {
                throw new CustomerNotFoundException(userId);
            }

            var orders = await this.unitOfWork.Orders.GetByCustomerIdAsync(customer.Id);
            return orders.Select(x => OrderModelDtoMapper.ToDto(x));
        }

        /// <summary>
        /// Creates a new order for a customer.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> CreateForCustomerAsync(string idempotencyKey, CreateOrderForCustomerDto newOrderData)
        {
            ArgumentNullException.ThrowIfNull(newOrderData);
            ArgumentException.ThrowIfNullOrEmpty(idempotencyKey);

            var customer = await this.unitOfWork.Customers.GetAsync(newOrderData.CustomerId!.Value);
            if (customer == null)
            {
                throw new CustomerNotFoundException(newOrderData.CustomerId!.Value);
            }

            return await CreateAsync(idempotencyKey, newOrderData, customer.Id);
        }

        /// <summary>
        /// Creates a new order for the authenticated customer.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <param name="userId">The userId.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> CreateAsync(string idempotencyKey, CreateOrderDto newOrderData, string userId)
        {
            ArgumentNullException.ThrowIfNull(newOrderData);
            ArgumentException.ThrowIfNullOrEmpty(idempotencyKey);

            var customer = await this.unitOfWork.Customers.GetByUserIdAsync(userId);
            if (customer == null)
            {
                throw new CustomerNotFoundException(userId);
            }

            return await CreateAsync(idempotencyKey, newOrderData, customer.Id);
        }

        private async Task<OrderDto> CreateAsync(string idempotencyKey, CreateOrderDto newOrderData, Guid customerId)
        {
            var (existingOrder, idempotencyKeyValue) = await this.ValidateIdempotencyKey(idempotencyKey);
            if (existingOrder != null)
            {
                return existingOrder;
            }

            var books = await this.ValidateBooks(newOrderData);
            var booksMap = books.ToDictionary(x => x.Id);
            ValidateStock(newOrderData, booksMap);

            return await ProcessTransaction(idempotencyKeyValue, newOrderData, booksMap, customerId);
        }

        private async Task<OrderDto> ProcessTransaction(Guid idempotencyKey, CreateOrderDto newOrderData, Dictionary<Guid, Book> booksMap, Guid customerId)
        {
            await using (var transaction = await this.unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Create the order with Pending status.
                    var orderToCreate = OrderModelDtoMapper.ToModel(newOrderData, idempotencyKey, customerId);
                    this.unitOfWork.Orders.Add(orderToCreate);
                    await this.unitOfWork.SaveChangesAsync();

                    var createdOrder = await this.unitOfWork.Orders.GetAsync(orderToCreate.Id, true);

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
                            book.Stock -= item.Quantity!.Value;
                        }
                    }

                    // Update the order status to Processed and save all the changes.
                    createdOrder.UpdateStatus(OrderStatus.Processed, false);
                    await this.unitOfWork.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return OrderModelDtoMapper.ToDto(createdOrder);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        private static void ValidateStock(CreateOrderDto newOrderData, Dictionary<Guid, Book> booksMap)
        {
            // Check if there is sufficient stock
            var insufficientStockItems = new List<InsufficientStockItem>();
            foreach (var item in newOrderData.OrderItems)
            {
                if (booksMap.TryGetValue(item.BookId!.Value, out var book))
                {
                    if (book.Stock < item.Quantity!.Value)
                    {
                        insufficientStockItems.Add(new InsufficientStockItem(book.Id, book.Isbn, book.Name, book.Stock));
                    }
                }
            }

            if (insufficientStockItems.Count > 0)
            {
                throw new InsufficientStockException(insufficientStockItems);
            }
        }

        private async Task<IEnumerable<Book>> ValidateBooks(CreateOrderDto newOrderData)
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
                var duplicateBooks = await this.unitOfWork.Books.GetAsync(duplicateBookIds);
                var duplicateItems = duplicateBooks.Select(book => new DuplicateItem(book.Id, book.Isbn, book.Name));
                throw new DuplicateItemException(duplicateItems);
            }

            // Check if all the books exist
            var books = await this.unitOfWork.Books.GetAsync(bookIds, true);
            var notFoundBookIds = bookIds.Except(books.Select(x => x.Id)).ToList();
            if (notFoundBookIds.Count > 0)
            {
                throw new BookNotFoundException(notFoundBookIds);
            }

            return books;
        }

        private async Task<(OrderDto? ExistingOrder, Guid IdempotencyKey)> ValidateIdempotencyKey(string idempotencyKey)
        {
            if (idempotencyKey.Length > 50 
                || !Guid.TryParse(idempotencyKey, out var idempotencyKeyValue) ||
                idempotencyKeyValue == Guid.Empty)
            {
                throw new ArgumentException("Invalid idempotency key provided.", nameof(idempotencyKey));
            }

            var duplicateOrder = await this.unitOfWork.Orders.GetByIdempotencyKeyAsync(idempotencyKeyValue);
            if (duplicateOrder != null)
            {
                if (duplicateOrder.CreatedAt <= DateTime.UtcNow.AddMinutes(-5))
                {
                    throw new IdempotencyKeyExpiredException(duplicateOrder.Id, idempotencyKeyValue);
                }
                else if (duplicateOrder.Status == OrderStatus.Pending)
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
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateStatusAsync(Guid id, UpdateOrderStatusDto updateData, bool isAdmin)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var order = await this.unitOfWork.Orders.GetAsync(id, true);
            if (order == null)
            {
                throw new OrderNotFoundException(id);
            }

            if (!order.UpdateStatus(updateData.Status!.Value, !isAdmin))
            {
                throw new InvalidOrderStatusChangeException(order.Id, order.Status, updateData.Status!.Value);
            }
            else if (order.Status == OrderStatus.Cancelled)
            {
                // Update stock for books in the cancelled order
                var bookIdsToUpdate = order.BookOrders.Select(x => x.BookId).ToHashSet();
                if (bookIdsToUpdate.Count > 0)
                {
                    var booksToUpdate = await this.unitOfWork.Books.GetAsync(bookIdsToUpdate, true);
                    var booksToUpdateMap = booksToUpdate.ToDictionary(x => x.Id);
                    foreach (var item in order.BookOrders)
                    {
                        if (booksToUpdateMap.TryGetValue(item.BookId, out var book))
                        {
                            book.Stock += item.Quantity;
                        }
                    }
                }
            }

            await this.unitOfWork.SaveChangesAsync();
        }
    }
}