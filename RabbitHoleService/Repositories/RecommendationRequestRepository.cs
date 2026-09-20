using Microsoft.EntityFrameworkCore;
using RabbitHoleService.Models;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Repositories
{
    /// <summary>
    /// The recommendation request repository.
    /// </summary>
    public class RecommendationRequestRepository : IRecommendationRequestRepository
    {
        private readonly BookStoreContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationRequestRepository" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RecommendationRequestRepository(BookStoreContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets the recommendation request.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="trackChanges">A value indicating whether changes should be tracked.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recommendation request.</returns>
        public async Task<RecommendationRequest?> GetRequestAsync(Guid correlationId, bool trackChanges = false, CancellationToken cancellationToken = default)
        {
            IQueryable<RecommendationRequest> query = this.context.RecommendationRequests;
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            var request = await query.FirstOrDefaultAsync(r => r.CorrelationId == correlationId, cancellationToken);
            return request;
        }

        /// <summary>
        /// Adds a recommendation request to the context.
        /// </summary>
        /// <param name="request">The recommendation request to add.</param>
        public void AddRequest(RecommendationRequest request)
        {
            this.context.RecommendationRequests.Add(request);
        }

        /// <summary>
        /// Deletes the expired recommendation requests.
        /// </summary>
        /// <param name="cutoff">The cutoff.</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>A task representing the delete operation.</returns>
        public async Task DeleteExpiredRequestsAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
        {
            await this.context.RecommendationRequests.Where(t => t.RequestedAt <= cutoff).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
