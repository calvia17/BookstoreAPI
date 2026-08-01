using Microsoft.AspNetCore.Identity;
using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The staff service.
    /// </summary>
    public class StaffService : BasePersonService<Staff, StaffDto>, IStaffService
    {
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Initializes the staff service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userManager">The user manager.</param>
        public StaffService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
            : base(unitOfWork, () => unitOfWork.Staff, person => StaffModelDtoMapper.ToDto((Staff)person), userManager)
        {
            this.unitOfWork = unitOfWork;
        }

        protected override Staff CreateEntity(RegisterUserDto registerUserDto, string userId)
        {
            return new Staff(registerUserDto.Name, registerUserDto.PhoneNumber, registerUserDto.Email, ((RegisterStaffDto)registerUserDto).Salary!.Value) { UserId = userId };
        }

        /// <summary>
        /// Updates the salary.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="updateData">The update data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the update operation.</returns>
        public async Task UpdateSalaryAsync(Guid id, UpdateSalaryDto updateData, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            var staff = await this.unitOfWork.Staff.GetAsync(id, true, cancellationToken);
            if (staff == null)
            {
                throw new PersonNotFoundException<Staff>(id);
            }

            staff.Salary = updateData.Salary!.Value;
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
