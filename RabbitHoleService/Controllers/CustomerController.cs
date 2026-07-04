using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Objects;
using RabbitHoleService.Services;
using System.Security.Claims;

namespace RabbitHoleService.Controllers
{
    /// <summary>
    /// The customer controller.
    /// </summary>
    [Route("api/customers")]
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
        [HttpGet("{id:guid}")]
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
            catch (PersonNotFoundException<Customer> ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.PersonId
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
        public async Task<ActionResult<CustomerDto>> GetMe()
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
            catch (PersonNotFoundException<Customer> ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.PersonId
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
        public async Task<ActionResult<IEnumerable<CustomerDto>>> FindCustomers([FromQuery] PersonSearchRequestDto request)
        {
            try
            {
                var customers = await this.customerService.FindPersonsAsync(request);
                return Ok(customers);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a customer with account registration.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <returns>The added customer.</returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDto>> RegisterCustomer([FromBody] RegisterUserDto registerData)
        {
            try
            {
                var createdCustomer = await this.customerService.RegisterAsync(registerData, RoleType.Customer);
                return CreatedAtAction(nameof(this.GetCustomer), new { id = createdCustomer.Id }, createdCustomer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (RegistrationFailedException)
            {
                return Conflict(new
                {
                    Title = "Registration Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Registration failed."
                });
            }
        }

        /// <summary>
        /// Adds a guest customer (guest checkout and in-store customers).
        /// </summary>
        /// <param name="newCustomerData">The new customer data.</param>
        /// <returns>The added customer.</returns>
        [AllowAnonymous]
        [HttpPost("guest")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDto>> AddGuestCustomer([FromBody] ContactInfoDto newCustomerData)
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
                    Title = "Guest Customer Creation Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
                });
            }
            catch (UsedEmailException ex)
            {
                return Conflict(new
                {
                    Title = "Guest Customer Creation Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message,
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
        [HttpPatch("{id:guid}")]
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
            catch (PersonNotFoundException<Customer>)
            {
                return Conflict(new
                {
                    Title = "Update Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Update failed.",
                });
            }
            catch (UpdatePersonFailedException)
            {
                return Conflict(new
                {
                    Title = "Update Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Update failed.",
                });
            }
        }

        /// <summary>
        /// Updates the account details.
        /// </summary>
        /// <param name="updateData">The update data.</param>
        /// <returns>The result of the update operation.</returns>
        [Authorize]
        [HttpPatch("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateContactInfoDto updateData)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Enum.TryParse<RoleType>(User.FindFirstValue(ClaimTypes.Role), true, out var role))
            {
                return Unauthorized();
            }

            try
            {
                await this.customerService.UpdateAccountAsync(userId, updateData);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Customer>)
            {
                return Conflict(new
                {
                    Title = "Update Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Update failed.",
                });
            }
            catch (UpdatePersonFailedException)
            {
                return Conflict(new
                {
                    Title = "Update Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Update failed.",
                });
            }
        }

        /// <summary>
        /// Updates the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The result of the change password operation.</returns>
        [Authorize]
        [HttpPatch("me/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePasswordAsync([FromBody] PasswordDto password)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                await this.customerService.UpdatePasswordAsync(userId, password);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UpdatePasswordFailedException ex)
            {
                return BadRequest(new
                {
                    Title = "Password Update Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a customer.
        /// </summary>
        /// <param name="id">The customer id.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpDelete("{id:guid}")]
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
            catch (PersonNotFoundException<Customer> ex)
            {
                return NotFound(new
                {
                    Title = "Customer Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CustomerId = ex.PersonId
                });
            }
        }

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>The result of the logout operation.</returns>
        [Authorize]
        [HttpDelete("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null && Enum.TryParse<RoleType>(User.FindFirstValue(ClaimTypes.Role), true, out var role))
                {
                    await this.customerService.DeleteAccountAsync(userId);
                }

                return Ok();
            }
            catch (ArgumentException)
            {
                return Ok();
            }
            catch (PersonNotFoundException<Customer>)
            {
                return Ok();
            }
        }
    }
}
