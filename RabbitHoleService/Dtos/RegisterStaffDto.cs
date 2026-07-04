using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The register staff dto.
    /// </summary>
    public class RegisterStaffDto : RegisterUserDto
    {
        /// <summary>
        /// Gets the salary.
        /// </summary>
        [Required(ErrorMessage = "The salary is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "The salary must be a positive number.")]
        public decimal? Salary { get; init; }
    }
}
