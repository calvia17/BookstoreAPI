using Microsoft.AspNetCore.Identity;
using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The customer service.
    /// </summary>
    public class CustomerService : BasePersonService<Customer, CustomerDto>, ICustomerService
    {
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Initializes the customer service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userManager">The user manager.</param>
        public CustomerService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
            : base(unitOfWork, () => unitOfWork.Customers, person => CustomerModelDtoMapper.ToDto((Customer)person), userManager)
        {
            this.unitOfWork = unitOfWork;
        }

        protected override Customer CreateEntity(RegisterUserDto registerUserDto, string userId)
        {
            return new Customer(registerUserDto.Name, registerUserDto.PhoneNumber, registerUserDto.Email) { UserId = userId };
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="newCustomerData">The new customer data.</param>
        /// <returns>The book.</returns>
        public async Task<CustomerDto> CreateAsync(ContactInfoDto? newCustomerData)
        {
            ArgumentNullException.ThrowIfNull(newCustomerData);
            var customerByPhone = await this.unitOfWork.Customers.GetByPhoneAsync(newCustomerData.PhoneNumber);
            if (customerByPhone != null)
            {
                throw new UsedPhoneException(newCustomerData.PhoneNumber);
            }

            var customerByEmail = await this.unitOfWork.Customers.GetByEmailAsync(newCustomerData.Email);
            if (customerByEmail != null)
            {
                throw new UsedEmailException(newCustomerData.Email);
            }

            var customerToCreate = CustomerModelDtoMapper.ToModel(newCustomerData);
            this.unitOfWork.Customers.Add(customerToCreate);
            await this.unitOfWork.SaveChangesAsync();
            return CustomerModelDtoMapper.ToDto(customerToCreate);
        }
    }
}