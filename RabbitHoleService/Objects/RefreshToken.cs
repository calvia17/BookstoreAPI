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
        /// Gets or sets the family id. This is used to group refresh tokens that belong to the same user and device.
        /// </summary>
        public Guid FamilyId { get; set; }

        /// <summary>
        /// Gets or sets the JWT id.
        /// </summary>
        public Guid JwtId { get; set; }

        /// <summary>
        /// Gets or sets the JWT expiry date.
        /// </summary>
        public DateTimeOffset JwtExpiry { get; set; }

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
        /// Gets or sets a value indicating whether the refresh token has been used.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshToken" /> class.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="expiryDate">The expiry date.</param>
        /// <param name="familyId">The family id.</param>
        /// <param name="jwtId">The jwt id.</param>
        public RefreshToken(string token, string userId, DateTimeOffset expiryDate, Guid familyId, Guid jwtId, DateTimeOffset jwtExpiry)
        {
            this.Token = token;
            this.UserId = userId;
            this.ExpiryDate = expiryDate;
            this.FamilyId = familyId;
            this.JwtId = jwtId;
            this.JwtExpiry = jwtExpiry;
        }
    }
}
