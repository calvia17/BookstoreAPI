using Microsoft.EntityFrameworkCore.Storage;
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Data
{
    /// <summary>
    /// The unit of work interface.
    /// </summary>
    public interface IUnitOfWork
    {
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
        public ICustomerRepository Customers { get; }

        /// <summary>
        /// The orders.
        /// </summary>
        public IOrderRepository Orders { get; }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns>A task representing the transaction start.</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns>The task representing the save operation.</returns>
        Task SaveChangesAsync();
    }
}
