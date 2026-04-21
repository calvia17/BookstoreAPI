using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Services;
using System.Security.Claims;

namespace RabbitHoleService.Controllers
{
    /// <summary>
    /// The customer controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService customerService;

        /// <summary>
        /// Initialises the customer controller.
        /// </summary>
        /// <param name="customerService">The customer service.</param>
        public CustomerController(ICustomerService customerService)
        {
            this.customerService = customerService;
        }

        /// <summary>
        /// Gets all the customers.
        /// </summary>
        /// <returns>The customers.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
        {
            var customers = await this.customerService.GetAllAsync();
            return Ok(customers);
        }

        /// <summary>
        /// Gets a customer by id.
        /// </summary>
        /// <param name="id">The customer id.</param>
        /// <returns>The customer.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomer([FromRoute] Guid id)
        {
            try
            {
                var customer = await this.customerService.GetAsync(id);
                return Ok(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
        }

        /// <summary>
        /// Gets the authenticated customer.
        /// </summary>
        /// <returns>The customer.</returns>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomer()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var customer = await this.customerService.GetByUserIdAsync(userId);
                return Ok(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
        }

        /// <summary>
        /// Finds customers that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The customers.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> FindCustomers([FromQuery] CustomerSearchRequestDto request)
        {
            try
            {
                var customers = await this.customerService.FindCustomersAsync(request);
                return Ok(customers);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a customer (guest checkout and in-store customers).
        /// </summary>
        /// <param name="newCustomerData">The new customer data.</param>
        /// <returns>The added customer.</returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDto>> AddCustomer([FromBody] ContactInfoDto newCustomerData)
        {
            try
            {
                var createdCustomer = await this.customerService.CreateAsync(newCustomerData);
                return CreatedAtAction(nameof(this.GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UsedPhoneException ex)
            {
                return Conflict(new
                {
                    Title = "Phone number already in use",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    Phone = ex.Phone
                });
            }
            catch (UsedEmailException ex)
            {
                return Conflict(new
                {
                    Title = "Email already in use",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                    Email = ex.Email
                });
            }
        }

        /// <summary>
        /// Updates a customer.
        /// </summary>
        /// <param name="id">The customer id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCustomer([FromRoute] Guid id, [FromBody] UpdateContactInfoDto updateData)
        {
            try
            {
                await this.customerService.UpdateAsync(id, updateData);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">The customer id.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCustomer([FromRoute] Guid id)
        {
            try
            {
                await this.customerService.DeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (CustomerNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.CustomerId
                });
            }
        }
    }
}
