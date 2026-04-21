using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The refresh token class.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// The id.
        /// </summary>
        [Key]
        public Guid Id { get; init; }

        /// <summary>
        /// The token.
        /// </summary>
        [Required(ErrorMessage = "The refresh token is required.")]
        [StringLength(64, MinimumLength = 1, ErrorMessage = "The refresh token must be between 1 and 64 characters.")]
        public string Token { get; set; }

        /// <summary>
        /// The user id.
        /// </summary>
        [Required(ErrorMessage = "The user id is required.")]
        public string UserId { get; set; }

        /// <summary>
        /// The expiry date. 
        /// </summary>
        [Required(ErrorMessage = "The expiry date is required.")]
        public DateTimeOffset ExpiryDate { get; set; }

        /// <summary>
        /// The user.
        /// </summary>
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshToken" /> class.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="expiryDate">The expiry date.</param>
        public RefreshToken(string token, string userId, DateTimeOffset expiryDate)
        {
            this.Token = token;
            this.UserId = userId;
            this.ExpiryDate = expiryDate;
        }
    }
}
