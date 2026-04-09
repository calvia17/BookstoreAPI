using RabbitHoleService.Objects;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The update order status dto.
    /// </summary>
    public class UpdateOrderStatusDto
    {
        /// <summary>
        /// Gets the order status.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "The order status is required.")]
        [EnumDataType(typeof(OrderStatus))]
        public required OrderStatus? Status { get; init; }
    }
}