using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos.Orders
{
    /// <summary>
    /// The create order for customer dto.
    /// </summary>
    public class CreateOrderForCustomerDto : CreateOrderDto
    {
        /// <summary>
        /// Gets the customer id.
        /// </summary>
        [Required(ErrorMessage = "The customer id is required.")]
        public required Guid? CustomerId { get; init; }
    }
}