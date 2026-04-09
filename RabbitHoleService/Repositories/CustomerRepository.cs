using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Dtos;
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
            var customers = await this.context.Customers.AsNoTracking().ToListAsync();
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

            var customer = await customersQuery.FirstOrDefaultAsync(c => c.Id == id);
            return customer;
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="newCustomerData">The new customer.</param>
        /// <returns>The customer.</returns>
        public async Task<Customer> AddAsync(Customer newCustomerData)
        {
            ArgumentNullException.ThrowIfNull(newCustomerData, nameof(newCustomerData));

            var createdCustomer = this.context.Customers.Add(newCustomerData);
            await this.context.SaveChangesAsync();
            return createdCustomer.Entity;
        }

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="updatedCustomer">The updated customer.</param>
        /// <returns>The task.</returns>
        public async Task UpdateAsync(Customer updatedCustomer)
        {
            ArgumentNullException.ThrowIfNull(updatedCustomer, nameof(updatedCustomer));
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns>The task.</returns>
        public async Task DeleteAsync(Customer customer)
        {
            ArgumentNullException.ThrowIfNull(customer, nameof(customer));

            this.context.Customers.Remove(customer);
            await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Finds customers that match a certain criteria.
        /// </summary>
        /// <param name="email">The name.</param>
        /// <param name="name">The phone.</param>
        /// <param name="phone">The email.</param>
        /// <returns>The customers.</returns>
        public async Task<IEnumerable<Customer>> FindCustomersAsync(string? name, string? phone, string? email)
        {
            var customers = this.context.Customers.AsNoTracking();
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