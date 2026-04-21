using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The refresh token dto.
    /// </summary>
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "The refresh token is required.")]
        [MinLength(1, ErrorMessage = "The refresh token cannot be empty.")]
        public required string RefreshToken { get; init; }
    }
}
