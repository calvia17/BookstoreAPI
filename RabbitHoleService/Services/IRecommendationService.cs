using RabbitHoleService.Dtos.Recommendations;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The recommendation service interface.
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Requests the recommendations for the books in the book shelf image.
        /// </summary>
        /// <param name="image">The book shelf image.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recommendation request.</returns>
        Task<RecommendationRequestDto> RequestRecommendationsAsync(IFormFile image, CancellationToken cancellationToken);

        /// <summary>
        /// Processes the recommendation result message.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="result">The recommendation result message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the processing operation.</returns>
        Task ProcessRecommendationsResultAsync(Guid correlationId, RecommendationResultMessageDto result, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the recommendations for the given correlation id.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recommendation result.</returns>
        Task<RecommendationResultDto> RetrieveRecommendationsAsync(Guid correlationId, CancellationToken cancellationToken);
    }
}
