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
        /// Gets all the persons.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The persons.</returns>
        public Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public Task<CustomerDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the person by the user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public Task<CustomerDto> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers a new person.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="role">The role.</param>
        /// <returns>The person.</returns>
        public Task<CustomerDto> RegisterAsync(RegisterUserDto registerData, RoleType role, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="newCustomerData">The customer to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The customer.</returns>
        Task<CustomerDto> CreateAsync(ContactInfoDto newCustomerData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        public Task UpdateAsync(Guid id, UpdateContactInfoDto updateData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the account details.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A value indicating whether the update was successful.</returns>
        public Task UpdateAccountAsync(string userId, UpdateContactInfoDto updateData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="passwordData">The password data.</param>
        public Task UpdatePasswordAsync(string userId, PasswordDto passwordData);

        /// <summary>
        /// Deletes a person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the delete operation.</returns>
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="person">The person associated with the user account.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the delete operation.</returns>
        public Task DeleteAccountAsync(string userId, IPerson? person = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds persons that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The persons.</returns>
        public Task<IEnumerable<CustomerDto>> FindPersonsAsync(PersonSearchRequestDto request, CancellationToken cancellationToken = default);
    }
}
