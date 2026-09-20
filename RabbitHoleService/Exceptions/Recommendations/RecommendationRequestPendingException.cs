namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The recommendation request pending exception.
    /// </summary>
    public class RecommendationRequestPendingException : BaseApplicationException
    {
        /// <summary>
        /// Gets the correlation id.
        /// </summary>
        public Guid CorrelationId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationRequestPendingException" /> class.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        public RecommendationRequestPendingException(Guid correlationId)
            : base($"Recommendation request is still being processed.")
        {
            this.CorrelationId = correlationId;
        }
    }
}
