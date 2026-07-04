using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The update salary dto.
    /// </summary>
    public class UpdateSalaryDto
    {
        /// <summary>
        /// Gets the salary.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "The salary must be a positive number.")]
        public decimal? Salary { get; init; }
    }
}
