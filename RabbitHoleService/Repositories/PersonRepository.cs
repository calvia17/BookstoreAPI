using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The base person repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public class PersonRepository<TEntity> : IPersonRepository<TEntity>
        where TEntity : class, IPerson
    {
        private readonly BookStoreContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRepository{TEntity}" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PersonRepository(BookStoreContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all the persons.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The persons.</returns>
        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var persons = await this.context.Set<TEntity>().AsNoTracking().Where(c => !c.IsDeleted).ToListAsync(cancellationToken);
            return persons;
        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public async Task<TEntity?> GetAsync(Guid id, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> personsQuery = this.context.Set<TEntity>();
            if (!trackChanges)
            {
                personsQuery = personsQuery.AsNoTracking();
            }

            var person = await personsQuery.FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id, cancellationToken);
            return person;
        }

        /// <summary>
        /// Gets the person by the user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public async Task<TEntity?> GetByUserIdAsync(string userId, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(userId);
            IQueryable<TEntity> personsQuery = this.context.Set<TEntity>();
            if (!trackChanges)
            {
                personsQuery = personsQuery.AsNoTracking();
            }

            var person = await personsQuery.FirstOrDefaultAsync(c => !c.IsDeleted && c.UserId == userId, cancellationToken);
            return person;
        }

        /// <summary>
        /// Gets the person by the phone number.
        /// </summary>
        /// <param name="phone">The phone number.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public async Task<TEntity?> GetByPhoneAsync(string phone, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(phone);
            IQueryable<TEntity> personsQuery = this.context.Set<TEntity>();
            if (!trackChanges)
            {
                personsQuery = personsQuery.AsNoTracking();
            }

            var person = await personsQuery.FirstOrDefaultAsync(c => !c.IsDeleted && c.PhoneNumber == phone, cancellationToken);
            return person;
        }

        /// <summary>
        /// Gets the person by the email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The person.</returns>
        public async Task<TEntity?> GetByEmailAsync(string email, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(email);
            IQueryable<TEntity> personsQuery = this.context.Set<TEntity>();
            if (!trackChanges)
            {
                personsQuery = personsQuery.AsNoTracking();
            }

            var person = await personsQuery.FirstOrDefaultAsync(c => !c.IsDeleted && c.Email == email, cancellationToken);
            return person;
        }

        /// <summary>
        /// Creates a new person.
        /// </summary>
        /// <param name="newPersonData">The new person.</param>
        public void Add(TEntity newPersonData)
        {
            ArgumentNullException.ThrowIfNull(newPersonData);
            this.context.Set<TEntity>().Add(newPersonData);
        }

        /// <summary>
        /// Finds persons that match a certain criteria.
        /// </summary>
        /// <param name="email">The name.</param>
        /// <param name="name">The phone.</param>
        /// <param name="phone">The email.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The persons.</returns>
        public async Task<List<TEntity>> FindAsync(string? name, string? phone, string? email, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> persons = this.context.Set<TEntity>().AsNoTracking().Where(c => !c.IsDeleted);
            if (!string.IsNullOrEmpty(name))
            {
                persons = persons.Where(c => c.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                persons = persons.Where(c => c.PhoneNumber.StartsWith(phone));
            }
            if (!string.IsNullOrEmpty(email))
            {
                persons = persons.Where(c => c.Email.StartsWith(email));
            }

            return await persons.ToListAsync(cancellationToken);
        }
    }
}
