using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Mappers
{
    /// <summary>
    /// The staff model dto mapper.
    /// </summary>
    public class StaffModelDtoMapper : IPersonModelDtoMapper<Staff, StaffDto>
    {
        /// <summary>
        /// Maps a staff to its dto.
        /// </summary>
        /// <param name="staff">The staff model.</param>
        /// <returns>The dto.</returns>
        public static StaffDto ToDto(Staff staff)
        {
            ArgumentNullException.ThrowIfNull(staff);
            return new StaffDto()
            {
                Id = staff.Id,
                Name = staff.Name,
                PhoneNumber = staff.PhoneNumber!,
                Email = staff.Email!,
                Salary = staff.Salary
            };
        }
    }
}
