using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The recommendation request repository interface.
    /// </summary>
    public interface IRecommendationRequestRepository
    {
        /// <summary>
        /// Gets the recommendation request.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recommendation request.</returns>
        Task<RecommendationRequest?> GetRequestAsync(Guid correlationId, bool trackChanges = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a recommendation request to the context.
        /// </summary>
        /// <param name="request">The recommendation request to add.</param>
        void AddRequest(RecommendationRequest request);

        /// <summary>
        /// Deletes the expired recommendation requests.
        /// </summary>
        /// <param name="cutoff">The cutoff.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>A task representing the delete operation.</returns>
        Task DeleteExpiredRequestsAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);
    }
}
