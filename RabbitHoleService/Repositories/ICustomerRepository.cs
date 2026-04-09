using RabbitHoleService.Dtos;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The customer repository interface.
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Gets all the customers.
        /// </summary>
        /// <returns>The customers.</returns>
        Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The customer.</returns>
        Task<Customer?> GetAsync(Guid id, bool trackChanges = false);

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="newCustomerData">The new customer data.</param>
        /// <returns>The customer.</returns>
        Task<Customer> AddAsync(Customer newCustomerData);

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="updateData">The update customer data.</param>
        /// <returns>A task that represents the update operation.</returns>
        Task UpdateAsync(Customer updateData);

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="cusotmer">The cusotmer.</param>
        /// <returns>A task that represents the delete operation.</returns>
        Task DeleteAsync(Customer customer);

        /// <summary>
        /// Finds customers that match a certain criteria.
        /// </summary>
        /// <param name="email">The name.</param>
        /// <param name="name">The phone.</param>
        /// <param name="phone">The email.</param>
        /// <returns>The customers.</returns>
        Task<IEnumerable<Customer>> FindCustomersAsync(string? name, string? phone, string? email);
    }
}
