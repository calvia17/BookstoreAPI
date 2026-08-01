using Microsoft.EntityFrameworkCore.Storage;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Data
{
    /// <summary>
    /// The unit of work interface.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// The cached books.
        /// </summary>
        public ICachedBookRepository CachedBooks { get; }

        /// <summary>
        /// The books.
        /// </summary>
        public IBookRepository Books { get; }

        /// <summary>
        /// The genres.
        /// </summary>
        public IGenreRepository Genres { get; }

        /// <summary>
        /// The customers.
        /// </summary>
        public IPersonRepository<Customer> Customers { get; }

        /// <summary>
        /// The staff.
        /// </summary>
        public IPersonRepository<Staff> Staff { get; }

        /// <summary>
        /// The orders.
        /// </summary>
        public IOrderRepository Orders { get; }

        /// <summary>
        /// The refresh tokens.
        /// </summary>
        public IRefreshTokenRepository RefreshTokens { get; }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns>A task representing the transaction start.</returns>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task representing the save operation.</returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
