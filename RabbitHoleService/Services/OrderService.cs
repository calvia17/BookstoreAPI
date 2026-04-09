using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;
using System.Text.RegularExpressions;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The order service.
    /// </summary>
    public partial class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly IBookService bookService;
        private readonly ICustomerService customerService;

        /// <summary>
        /// Initializes the order service.
        /// </summary>
        /// <param name="orderRepository">The order repository.</param>
        /// <param name="bookService">The book service.</param>
        /// <param name="customerService">The customer service.</param>
        public OrderService(
            IOrderRepository orderRepository,
            IBookService bookService,
            ICustomerService customerService)
        {
            this.orderRepository = orderRepository;
            this.bookService = bookService;
            this.customerService = customerService;
        }

        /// <summary>
        /// Gets all the orders.
        /// </summary>
        /// <returns>The orders.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await this.orderRepository.GetAllAsync();
            var dtos = orders.Select(x => OrderModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> GetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var order = await this.orderRepository.GetAsync(id);
            if (order == null)
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
            // TODO: Add authorization
            if (customerId == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(customerId));
            }

            var orders = await this.orderRepository.GetByCustomerIdAsync(customerId);
            return orders.Select(x => OrderModelDtoMapper.ToDto(x));
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="newOrderData">The new order data.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <returns>The order.</returns>
        public async Task<OrderDto> CreateAsync(string idempotencyKey, CreateOrderDto newOrderData)
        {
            ArgumentNullException.ThrowIfNull(newOrderData);
            ArgumentException.ThrowIfNullOrEmpty(idempotencyKey);

            var (existingOrder, idempotencyKeyValue) = await this.ValidateIdempotencyKey(idempotencyKey);
            if (existingOrder != null)
            {
                return existingOrder;
            }

            var customer = await this.customerService.GetAsync(newOrderData.CustomerId!.Value);
            if (customer == null)
            {
                throw new CustomerNotFoundException(newOrderData.CustomerId!.Value);
            }

            var books = await this.ValidateBooks(newOrderData);
            var booksMap = books.ToDictionary(x => x.Id);
            ValidateStock(newOrderData, booksMap);

            return await ProcessTransaction(idempotencyKeyValue, newOrderData, booksMap);
        }

        /// <summary>
        /// Updates the order status.
        /// </summary>
        /// <param name="id">The order id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateStatusAsync(Guid id, UpdateOrderStatusDto updateData)
        {
            // TODO: Add authorization
            ArgumentNullException.ThrowIfNull(updateData);

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var order = await this.orderRepository.GetAsync(id, true);
            if (order == null)
            {
                throw new OrderNotFoundException(id);
            }

            if (!order.UpdateStatus(updateData.Status!.Value))
            {
                throw new InvalidOrderStatusChangeException(order.Id, order.Status, updateData.Status!.Value);
            }

            await this.orderRepository.UpdateAsync(order);
        }

        private async Task<OrderDto> ProcessTransaction(Guid idempotencyKey, CreateOrderDto newOrderData, Dictionary<Guid, BookDto> booksMap)
        {
            await using (var transaction = await this.orderRepository.BeginTransactionAsync())
            {
                try
                {
                    // Create the order
                    var orderToCreate = OrderModelDtoMapper.ToModel(newOrderData, idempotencyKey);

                    // Add books
                    var bookStockUpdates = new Dictionary<Guid, int>();
                    foreach (var item in newOrderData.OrderItems)
                    {
                        if (booksMap.TryGetValue(item.BookId!.Value, out var book))
                        {
                            var bookModel = BookModelDtoMapper.ToModel(book);
                            orderToCreate.AddBook(bookModel, item.Quantity!.Value);
                            bookStockUpdates[book.Id] = book.Stock - item.Quantity!.Value;
                        }
                    }

                    var createdOrder = await this.orderRepository.AddAsync(orderToCreate);

                    // Update stock
                    var bookUpdateDtos = bookStockUpdates.Select(x => new BookData(x.Key, new UpdateBookDto() { Stock = x.Value }));
                    await this.bookService.UpdateMultipleAsync(new UpdateMultipleBooksDto() { Books = bookUpdateDtos });

                    // Update status
                    createdOrder.UpdateStatus(OrderStatus.Processed, false);
                    await this.orderRepository.UpdateAsync(createdOrder);

                    await this.orderRepository.CommitTransactionAsync(transaction);
                    return OrderModelDtoMapper.ToDto(createdOrder);
                }
                catch
                {
                    await this.orderRepository.RollbackTransactionAsync(transaction);
                    throw;
                }
            }
        }

        private static void ValidateStock(CreateOrderDto newOrderData, Dictionary<Guid, BookDto> booksMap)
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

        private async Task<IEnumerable<BookDto>> ValidateBooks(CreateOrderDto newOrderData)
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
                var duplicateBooks = await this.bookService.GetAsync(duplicateBookIds);
                var duplicateItems = duplicateBooks.Select(book => new DuplicateItem(book.Id, book.Isbn, book.Name));
                throw new DuplicateItemException(duplicateItems);
            }

            // Check if all the books exist
            var books = await this.bookService.GetAsync(bookIds);
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

            var duplicateOrder = await this.orderRepository.GetByIdempotencyKeyAsync(idempotencyKeyValue);
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
    }
}