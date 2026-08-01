using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The staff service interface.
    /// </summary>
    public interface IStaffService
    {
        /// <summary>
        /// Gets all the persons.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The persons.</returns>
        public Task<IEnumerable<StaffDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public Task<StaffDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the person by the user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public Task<StaffDto> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers a new person.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <param name="role">The role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public Task<StaffDto> RegisterAsync(RegisterUserDto registerData, RoleType role, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        public Task UpdateAsync(Guid id, UpdateContactInfoDto updateData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the salary.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        public Task UpdateSalaryAsync(Guid id, UpdateSalaryDto updateData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="passwordData">The password data.</param>
        /// <returns>A task that represents the update operation.</returns>
        public Task UpdatePasswordAsync(string userId, PasswordDto passwordData);

        /// <summary>
        /// Deletes a person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the delete operation.</returns>
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds persons that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The persons.</returns>
        public Task<IEnumerable<StaffDto>> FindPersonsAsync(PersonSearchRequestDto request, CancellationToken cancellationToken = default);
    }
}
