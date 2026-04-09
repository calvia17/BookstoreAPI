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
        private readonly ICustomerRepository repository;

        /// <summary>
        /// Initializes the customer service.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public CustomerService(ICustomerRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Gets all the customers.
        /// </summary>
        /// <returns>The customers.</returns>
        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            var customers = await this.repository.GetAllAsync();
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

            var customer = await this.repository.GetAsync(id);
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
            var duplicateCustomer = this.GetAllAsync().Result.FirstOrDefault(x =>
                string.Equals(newCustomerData.PhoneNumber, x.PhoneNumber, StringComparison.OrdinalIgnoreCase));
            if (duplicateCustomer != null)
            {
                throw new UsedPhoneException(newCustomerData.PhoneNumber);
            }

            duplicateCustomer = this.GetAllAsync().Result.FirstOrDefault(x =>
                string.Equals(newCustomerData.Email, x.Email, StringComparison.OrdinalIgnoreCase));
            if (duplicateCustomer != null)
            {
                throw new UsedEmailException(newCustomerData.Email);
            }

            var customerToCreate = CustomerModelDtoMapper.ToModel(newCustomerData);
            var createdCustomer = await this.repository.AddAsync(customerToCreate);
            return CustomerModelDtoMapper.ToDto(createdCustomer);
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

            var customer = await this.repository.GetAsync(id, true);
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

            await this.repository.UpdateAsync(customer);
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

            var customer = await this.repository.GetAsync(id);
            if (customer == null)
            {
                throw new CustomerNotFoundException(id);
            }

            await this.repository.DeleteAsync(customer);
        }

        /// <summary>
        /// Finds customers that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The customers.</returns>
        public async Task<IEnumerable<CustomerDto>> FindCustomersAsync(CustomerSearchRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var customers = await this.repository.FindCustomersAsync(request.Name, request.PhoneNumber, request.Email);
            return customers.Select(CustomerModelDtoMapper.ToDto).ToList();
        }
    }
}