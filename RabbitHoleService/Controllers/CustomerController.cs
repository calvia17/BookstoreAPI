using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The customers.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers(CancellationToken cancellationToken)
        {
            var customers = await this.customerService.GetAllAsync(cancellationToken);
            return Ok(customers);
        }

        /// <summary>
        /// Gets a customer by id.
        /// </summary>
        /// <param name="id">The customer id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The customer.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetCustomer([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await this.customerService.GetAsync(id, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The customer.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetMe(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var customer = await this.customerService.GetByUserIdAsync(userId, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="request">The search request.</param>
        /// <returns>The customers.</returns>
        [EnableRateLimiting("ReadPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> FindCustomers([FromQuery] PersonSearchRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var customers = await this.customerService.FindPersonsAsync(request, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added customer.</returns>
        [EnableRateLimiting("UserManagementPolicy")]
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDto>> RegisterCustomer([FromBody] RegisterUserDto registerData, CancellationToken cancellationToken)
        {
            try
            {
                var createdCustomer = await this.customerService.RegisterAsync(registerData, RoleType.Customer, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added customer.</returns>
        [EnableRateLimiting("UserManagementPolicy")]
        [AllowAnonymous]
        [HttpPost("guest")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerDto>> AddGuestCustomer([FromBody] ContactInfoDto newCustomerData, CancellationToken cancellationToken)
        {
            try
            {
                var createdCustomer = await this.customerService.CreateAsync(newCustomerData, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="updateData">The data to update.</param>
        /// <returns>No content.</returns>
        [EnableRateLimiting("UserManagementPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCustomer([FromRoute] Guid id, [FromBody] UpdateContactInfoDto updateData, CancellationToken cancellationToken)
        {
            try
            {
                await this.customerService.UpdateAsync(id, updateData, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the update operation.</returns>
        [EnableRateLimiting("UserManagementPolicy")]
        [Authorize]
        [HttpPatch("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateContactInfoDto updateData, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !Enum.TryParse<RoleType>(User.FindFirstValue(ClaimTypes.Role), true, out var role))
            {
                return Unauthorized();
            }

            try
            {
                await this.customerService.UpdateAccountAsync(userId, updateData, cancellationToken);
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
        [EnableRateLimiting("UserManagementPolicy")]
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [EnableRateLimiting("UserManagementPolicy")]
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCustomer([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await this.customerService.DeleteAsync(id, cancellationToken);
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the logout operation.</returns>
        [EnableRateLimiting("UserManagementPolicy")]
        [Authorize]
        [HttpDelete("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null && Enum.TryParse<RoleType>(User.FindFirstValue(ClaimTypes.Role), true, out var role))
                {
                    await this.customerService.DeleteAccountAsync(userId, cancellationToken: cancellationToken);
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
