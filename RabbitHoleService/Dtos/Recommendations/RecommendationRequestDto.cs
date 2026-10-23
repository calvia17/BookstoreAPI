namespace RabbitHoleService.Dtos.Recommendations
{
    /// <summary>
    /// The recommendation request dto.
    /// </summary>
    public class RecommendationRequestDto
    {
        /// <summary>
        /// Gets the correlation id.
        /// </summary>
        public Guid CorrelationId { get; init; }
    }
}
