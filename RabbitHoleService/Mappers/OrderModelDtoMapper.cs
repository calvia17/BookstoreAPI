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
    }
}