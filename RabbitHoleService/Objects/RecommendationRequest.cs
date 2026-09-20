using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The recommendation request.
    /// </summary>
    public class RecommendationRequest
    {
        /// <summary>
        /// Gets the correlation id.
        /// </summary>
        [Key]
        public Guid CorrelationId { get; init; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public RecommendationRequestStatus Status { get; set; }

        /// <summary>
        /// The extracted book isbns.
        /// </summary>
        public string? ExtractedIsbnsJson { get; set; }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public string? ResultJson { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Gets the requested at timestamp.
        /// </summary>
        public DateTimeOffset RequestedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationRequest" /> class.
        /// </summary>
        public RecommendationRequest() 
        {
            this.Status = RecommendationRequestStatus.Pending;
            this.RequestedAt = DateTimeOffset.UtcNow;
        }
    }
}
