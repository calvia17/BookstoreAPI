using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Mappers
{
    /// <summary>
    /// The order model dto mapper.
    /// </summary>
    public class OrderModelDtoMapper
    {
        /// <summary>
        /// Maps an order to its dto.
        /// </summary>
        /// <param name="order">The order model.</param>
        /// <returns>The dto.</returns>
        public static OrderDto ToDto(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            return new OrderDto()
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                TotalCost = order.TotalCost,
                CustomerId = order.CustomerId,
                OrderItems = order.BookOrders.Select(bo => new OrderItemDto
                {
                    BookId = bo.BookId,
                    Isbn = bo.Book.Isbn,
                    BookName = bo.Book.Name,
                    Quantity = bo.Quantity,
                    PriceAtPurchase = bo.PriceAtPurchase
                }).ToList(),
            };
        }

        /// <summary>
        /// Converts the dto into an order model.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <param name="idempotencyKey">The idempotency key.</param>
        /// <returns>The model.</returns>
        public static Order ToModel(CreateOrderDto dto, Guid idempotencyKey)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var order = new Order(idempotencyKey, dto.CustomerId!.Value);
            return order;
        }
    }
}