namespace RabbitHoleService.Exceptions.Recommendations
{
    /// <summary>
    /// The recommendation request failed exception.
    /// </summary>
    public class RecommendationRequestFailedException : BaseApplicationException
    {
        /// <summary>
        /// Gets the correlation id.
        /// </summary>
        public Guid CorrelationId { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationRequestFailedException" /> class.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="error">The error message.</param>
        public RecommendationRequestFailedException(Guid correlationId, string error)
            : base($"Recommendation request failed.")
        {
            this.CorrelationId = correlationId;
            this.Error = error;
        }
    }
}
