using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The customer service interface.
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Gets all the customers.
        /// </summary>
        /// <returns>The customers.</returns>
        Task<IEnumerable<CustomerDto>> GetAllAsync();

        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The customer.</returns>
        Task<CustomerDto> GetAsync(Guid id);

        /// <summary>
        /// Gets the customer by the user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The customer.</returns>
        Task<CustomerDto> GetByUserIdAsync(string userId);

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="newCustomerData">The customer to create.</param>
        /// <returns>The customer.</returns>
        Task<CustomerDto> CreateAsync(ContactInfoDto newCustomerData);

        /// <summary>
        /// Updates a customer.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateAsync(Guid id, UpdateContactInfoDto updateData);

        /// <summary>
        /// Updates the customer properties.
        /// </summary>
        /// <param name="updateData">The update data.</param>
        /// <param name="customer">The customer to update.</param>
        void UpdateProperties(UpdateContactInfoDto updateData, Customer customer);

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">The id.</param>=
        /// <returns>A task that represents the delete operation.</returns>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Finds customers that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The customers.</returns>
        Task<IEnumerable<CustomerDto>> FindCustomersAsync(CustomerSearchRequestDto request);
    }
}
