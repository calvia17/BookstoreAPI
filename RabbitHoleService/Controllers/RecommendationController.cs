using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Exceptions.Recommendations;
using RabbitHoleService.Services;

namespace RabbitHoleService.Controllers
{
    /// <summary>
    /// The recommendation controller.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService recommendationService;

        /// <summary>
        /// Initializes the recommendation controller.
        /// </summary>
        /// <param name="recommendationService">The recommendation service.</param>
        public RecommendationController(IRecommendationService recommendationService)
        {
            this.recommendationService = recommendationService;
        }

        /// <summary>
        /// Requests the recommendations for the books in the book shelf image.
        /// </summary>
        /// <param name="image">The image file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The correlation id.</returns>
        [EnableRateLimiting("RecommendationRequestPolicy")]
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestRecommendations(IFormFile image, CancellationToken cancellationToken)
        {
            try
            {
                var result = await this.recommendationService.RequestRecommendationsAsync(image, cancellationToken);
                return Ok(result);
            }
            catch (InvalidImageException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the recommendation result.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recommendation result.</returns>
        [EnableRateLimiting("RecommendationRetrievePolicy")]
        [AllowAnonymous]
        [HttpGet("{correlationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> RetrieveRecommendations(Guid correlationId, CancellationToken cancellationToken)
        {
            // This endpoint will be polled by the client until the recommendation result is completed or failed.
            // We cannot cache pending requests since that woould cause the client to receive pending status even when the result is completed/ failed.
            // Caching a completed/ failed result is unnecessary since the client will stop polling once the result is completed/ failed.
            // So caching it would only cause cache pollution since it won't be retrieved again with the same correlation id.
            try
            {
                var result = await this.recommendationService.RetrieveRecommendationsAsync(correlationId, cancellationToken);
                return Ok(result);
            }
            catch (RecommendationRequestNotFoundException ex)
            {
                return NotFound(new
                {
                    Title = "Book Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message,
                    CorrelationId = ex.CorrelationId
                });
            }
            catch (RecommendationRequestFailedException ex)
            {
                return UnprocessableEntity(new
                {
                    Title = "Recommendation Request Failed",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = ex.Message,
                    CorrelationId = ex.CorrelationId,
                    Error = ex.Error
                });
            }
            catch (RecommendationRequestPendingException ex)
            {
                return Accepted(Url.Action(nameof(RetrieveRecommendations), new { correlationId }), new
                {
                    Title = "Recommendation Request Pending",
                    Status = StatusCodes.Status202Accepted,
                    Detail = ex.Message,
                    CorrelationId = ex.CorrelationId
                });
            }
        }
    }
}
