namespace RabbitHoleService.Exceptions.Recommendations
{
    /// <summary>
    /// The recommendation request not found exception.
    /// </summary>
    public class RecommendationRequestNotFoundException : BaseApplicationException
    {
        /// <summary>
        /// Gets the correlation id.
        /// </summary>
        public Guid CorrelationId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationRequestNotFoundException" /> class.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        public RecommendationRequestNotFoundException(Guid correlationId)
            : base($"Recommendation request not found.")
        {
            this.CorrelationId = correlationId;
        }
    }
}
