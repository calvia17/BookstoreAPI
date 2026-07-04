using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Mappers
{
    /// <summary>
    /// The customer model dto mapper.
    /// </summary>
    public class CustomerModelDtoMapper : IPersonModelDtoMapper<Customer, CustomerDto>
    {
        /// <summary>
        /// Maps a customer to its dto.
        /// </summary>
        /// <param name="customer">The customer model.</param>
        /// <returns>The dto.</returns>
        public static CustomerDto ToDto(Customer customer)
        {
            ArgumentNullException.ThrowIfNull(customer);

            return new CustomerDto()
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email
            };
        }

        /// <summary>
        /// Converts the dto into a customer model.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns>The model.</returns>
        public static Customer ToModel(ContactInfoDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new Customer(dto.Name, dto.PhoneNumber, dto.Email);
        }
    }
}
