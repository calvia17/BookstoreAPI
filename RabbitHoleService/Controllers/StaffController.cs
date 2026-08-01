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
    /// The staff controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService staffService;

        /// <summary>
        /// Initialises the staff controller.
        /// </summary>
        /// <param name="staffService">The staff service.</param>
        public StaffController(IStaffService staffService)
        {
            this.staffService = staffService;
        }

        /// <summary>
        /// Gets all the staff.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The staff.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffDto>>> GetAllStaff(CancellationToken cancellationToken)
        {
            var staff = await this.staffService.GetAllAsync(cancellationToken);
            return Ok(staff);
        }

        /// <summary>
        /// Gets a staff member by id.
        /// </summary>
        /// <param name="id">The staff id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The staff member.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StaffDto>> GetStaff([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var staff = await this.staffService.GetAsync(id, cancellationToken);
                return Ok(staff);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Staff> ex)
            {
                return NotFound(new
                {
                    Title = "Staff Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    StaffId = ex.PersonId
                });
            }
        }

        /// <summary>
        /// Gets the authenticated staff member.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The staff member.</returns>
        [Authorize(Policy = "StaffOrAdmin")]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StaffDto>> GetMe(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var staff = await this.staffService.GetByUserIdAsync(userId, cancellationToken);
                return Ok(staff);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Staff> ex)
            {
                return NotFound(new
                {
                    Title = "Staff Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    StaffId = ex.PersonId
                });
            }
        }

        /// <summary>
        /// Finds staff that match a certain criteria.
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The staff members.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<StaffDto>>> FindStaff([FromQuery] PersonSearchRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var staff = await this.staffService.FindPersonsAsync(request, cancellationToken);
                return Ok(staff);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a staff member.
        /// </summary>
        /// <param name="registerData">The registration data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The added staff member.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<StaffDto>> RegisterStaff([FromBody] RegisterStaffDto registerData, CancellationToken cancellationToken)
        {
            try
            {
                var createdStaff = await this.staffService.RegisterAsync(registerData, RoleType.Staff, cancellationToken);
                return CreatedAtAction(nameof(this.GetStaff), new { id = createdStaff.Id }, createdStaff);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (RegistrationFailedException ex)
            {
                return Conflict(new
                {
                    Title = "Registration Failed",
                    Status = StatusCodes.Status409Conflict,
                    Detail = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a staff member.
        /// </summary>
        /// <param name="id">The staff id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStaff([FromRoute] Guid id, [FromBody] UpdateContactInfoDto updateData, CancellationToken cancellationToken)
        {
            try
            {
                await this.staffService.UpdateAsync(id, updateData, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Staff> ex)
            {
                return NotFound(new
                {
                    Title = "Staff Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    StaffId = ex.PersonId
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
        /// Updates the salary of a staff member.
        /// </summary>
        /// <param name="id">The staff id.</param>
        /// <param name="updateData">The data to update.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPatch("{id:guid}/salary")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSalary([FromRoute] Guid id, [FromBody] UpdateSalaryDto updateData, CancellationToken cancellationToken)
        {
            try
            {
                await this.staffService.UpdateSalaryAsync(id, updateData, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Staff> ex)
            {
                return NotFound(new
                {
                    Title = "Staff Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    StaffId = ex.PersonId
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

                await this.staffService.UpdatePasswordAsync(userId, password);
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
        /// Deletes a staff member.
        /// </summary>
        /// <param name="id">The staff id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>No content.</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteStaff([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await this.staffService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (PersonNotFoundException<Staff> ex)
            {
                return NotFound(new
                {
                    Title = "Staff Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    StaffId = ex.PersonId
                });
            }
        }
    }
}
