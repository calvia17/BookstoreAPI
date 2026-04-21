using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The customer repository.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly BookStoreContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerRepository" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public CustomerRepository(BookStoreContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets all the customers.
        /// </summary>
        /// <returns>The customers.</returns>
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            var customers = await this.context.Customers.AsNoTracking().Where(c => !c.IsDeleted).ToListAsync();
            return customers;
        }

        /// <summary>
        /// Gets the customer.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The customer.</returns>
        public async Task<Customer?> GetAsync(Guid id, bool trackChanges = false)
        {
            ArgumentNullException.ThrowIfNull(id, nameof(id));
            IQueryable<Customer> customersQuery = this.context.Customers;
            if (!trackChanges)
            {
                customersQuery = customersQuery.AsNoTracking();
            }

            var customer = await customersQuery.FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);
            return customer;
        }

        /// <summary>
        /// Gets the customer by the user id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The customer.</returns>
        public async Task<Customer?> GetByUserIdAsync(string userId, bool trackChanges = false)
        {
            IQueryable<Customer> customersQuery = this.context.Customers;
            if (!trackChanges)
            {
                customersQuery = customersQuery.AsNoTracking();
            }

            var customer = await customersQuery.FirstOrDefaultAsync(c => !c.IsDeleted && c.UserId == userId);
            return customer;
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="newCustomerData">The new customer.</param>
        public void Add(Customer newCustomerData)
        {
            ArgumentNullException.ThrowIfNull(newCustomerData, nameof(newCustomerData));
            this.context.Customers.Add(newCustomerData);
        }

        /// <summary>
        /// Finds customers that match a certain criteria.
        /// </summary>
        /// <param name="email">The name.</param>
        /// <param name="name">The phone.</param>
        /// <param name="phone">The email.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <returns>The customers.</returns>
        public async Task<List<Customer>> FindCustomersAsync(string? name, string? phone, string? email, bool trackChanges = false)
        {
            IQueryable<Customer> customers = this.context.Customers;
            if (!trackChanges)
            {
                customers = customers.AsNoTracking();
            }

            customers = customers.Where(c => !c.IsDeleted);
            if (!string.IsNullOrEmpty(name))
            {
                customers = customers.Where(c => c.Name.StartsWith(name));
            }
            if (!string.IsNullOrEmpty(phone))
            {
                customers = customers.Where(c => c.PhoneNumber.StartsWith(phone));
            }
            if (!string.IsNullOrEmpty(email))
            {
                customers = customers.Where(c => c.Email.StartsWith(email));
            }

            return await customers.ToListAsync();
        }
    }
}