using Microsoft.EntityFrameworkCore.Storage;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Data
{
    /// <summary>
    /// The unit of work class.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BookStoreContext context;
        
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
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cachedBooks">The cached book repository.</param>
        /// <param name="books">The book repository.</param>
        /// <param name="genres">The genre repository.</param>
        /// <param name="customers">The customer repository.</param>
        /// <param name="staff">The staff.</param>
        /// <param name="orders">The order repository.</param>
        /// <param name="refreshTokens">The refresh token repository.</param>
        public UnitOfWork(BookStoreContext context, ICachedBookRepository cachedBooks, IBookRepository books, IGenreRepository cachedGenres, IGenreRepository genres, IPersonRepository<Customer> customers, IPersonRepository<Staff> staff, IOrderRepository orders, IRefreshTokenRepository refreshTokens)
        {
            this.context = context;
            this.CachedBooks = cachedBooks;
            this.Books = books;
            this.Genres = genres;
            this.Customers = customers;
            this.Staff = staff;
            this.Orders = orders;
            this.RefreshTokens = refreshTokens;
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the transaction start.</returns>
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return await this.context.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task representing the save operation.</returns>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await this.context.SaveChangesAsync(cancellationToken);
        }
    }
}
