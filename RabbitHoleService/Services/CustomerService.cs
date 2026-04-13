using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The customer service.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Initializes the customer service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public CustomerService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Gets all the customers.
        /// </summary>
        /// <returns>The customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await this.unitOfWork.Customers.GetAllAsync();
            var dtos = customers.Select(x => CustomerModelDtoMapper.ToDto(x)).ToList();
            return dtos;
        }

        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The customer.</returns>
        public async Task<CustomerDto> GetAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var customer = await this.unitOfWork.Customers.GetAsync(id);
            if (customer == null)
            {
                throw new CustomerNotFoundException(id);
            }

            return CustomerModelDtoMapper.ToDto(customer);
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="newCustomerData">The new customer data.</param>
        /// <returns>The book.</returns>
        public async Task<CustomerDto> CreateAsync(CreateCustomerDto? newCustomerData)
        {
            ArgumentNullException.ThrowIfNull(newCustomerData, nameof(newCustomerData));
            var duplicateCustomer = await this.unitOfWork.Customers.FindCustomersAsync(null, newCustomerData.PhoneNumber, null);
            if (duplicateCustomer.Any())
            {
                throw new UsedPhoneException(newCustomerData.PhoneNumber);
            }

            duplicateCustomer = await this.unitOfWork.Customers.FindCustomersAsync(null, null, newCustomerData.Email);
            if (duplicateCustomer.Any())
            {
                throw new UsedEmailException(newCustomerData.Email);
            }

            var customerToCreate = CustomerModelDtoMapper.ToModel(newCustomerData);
            this.unitOfWork.Customers.Add(customerToCreate);
            await this.unitOfWork.SaveChangesAsync();
            return CustomerModelDtoMapper.ToDto(customerToCreate);
        }

        /// <summary>
        /// Updates a customer.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateAsync(Guid id, UpdateCustomerDto? updateData)
        {
            ArgumentNullException.ThrowIfNull(updateData, nameof(updateData));

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var customer = await this.unitOfWork.Customers.GetAsync(id, true);
            if (customer == null)
            {
                throw new CustomerNotFoundException(id);
            }

            if (!string.IsNullOrEmpty(updateData.Name))
            {
                customer.Name = updateData.Name;
            }

            if (!string.IsNullOrEmpty(updateData.PhoneNumber))
            {
                customer.PhoneNumber = updateData.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(updateData.Email))
            {
                customer.Email = updateData.Email;
            }

            await this.unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">The id.</param>=
        /// <returns>A task that represents the delete operation.</returns>
        public async Task DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var customer = await this.unitOfWork.Customers.GetAsync(id);
            if (customer == null)
            {
                throw new CustomerNotFoundException(id);
            }

            this.unitOfWork.Customers.Delete(customer);
            await this.unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Finds customers that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The customers.</returns>
        public async Task<IEnumerable<CustomerDto>> FindCustomersAsync(CustomerSearchRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var customers = await this.unitOfWork.Customers.FindCustomersAsync(request.Name, request.PhoneNumber, request.Email);
            return customers.Select(CustomerModelDtoMapper.ToDto).ToList();
        }
    }
}