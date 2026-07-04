using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The person repository interface.
    /// </summary>
    public interface IPersonRepository<TEntity>
        where TEntity : class, IPerson
    {
        /// <summary>
        /// Gets all the persons.
        /// </summary>
        /// <returns>The persons.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The person.</returns>
        Task<TEntity?> GetAsync(Guid id, bool trackChanges = false);

        /// <summary>
        /// Gets the person by the user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The person.</returns>
        Task<TEntity?> GetByUserIdAsync(string userId, bool trackChanges = false);

        /// <summary>
        /// Gets the person by the phone number.
        /// </summary>
        /// <param name="phone">The phone number.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The person.</returns>
        Task<TEntity?> GetByPhoneAsync(string phone, bool trackChanges = false);

        /// <summary>
        /// Gets the person by the email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The person.</returns>
        Task<TEntity?> GetByEmailAsync(string email, bool trackChanges = false);

        /// <summary>
        /// Creates a new person.
        /// </summary>
        /// <param name="newPersonData">The new person data.</param>
        void Add(TEntity newPersonData);

        /// <summary>
        /// Finds persons that match a certain criteria.
        /// </summary>
        /// <param name="email">The name.</param>
        /// <param name="name">The phone.</param>
        /// <param name="phone">The email.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The persons.</returns>
        Task<List<TEntity>> FindAsync(string? name, string? phone, string? email, bool trackChanges = false);
    }
}
