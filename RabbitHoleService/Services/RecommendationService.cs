using RabbitHoleService.Data;
using RabbitHoleService.Dtos;
using RabbitHoleService.Events;
using RabbitHoleService.Exceptions;
using RabbitHoleService.Exceptions.Recommendations;
using RabbitHoleService.Mappers;
using RabbitHoleService.Objects;
using SixLabors.ImageSharp;
using System.Text.Json;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The recommendation service.
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private const int MaxImageSize = 10 * 1024 * 1024; // 10 MB
        private static readonly string[] AllowedImageFormats = { ".jpeg", ".jpg", ".png" };
        private readonly IUnitOfWork unitOfWork;
        private readonly IBookEventPublisher eventPublisher;
        private readonly IBlobStorageService blobStorageService;

        /// <summary>
        /// Initializes the recommendation service.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="blobStorageService">The blob storage service.</param>
        public RecommendationService(IUnitOfWork unitOfWork, IBookEventPublisher eventPublisher, IBlobStorageService blobStorageService) 
        {
            this.unitOfWork = unitOfWork;
            this.eventPublisher = eventPublisher;
            this.blobStorageService = blobStorageService;
        }

        /// <summary>
        /// Requests the recommendations for the books in the book shelf image.
        /// </summary>
        /// <param name="image">The book shelf image.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recommendation request.</returns>
        public async Task<RecommendationRequestDto> RequestRecommendationsAsync(IFormFile image, CancellationToken cancellationToken)
        {
            await ValidateImage(image);

            // Create a new recommendation request and save it to the database.
            var recommendationRequest = new RecommendationRequest();
            this.unitOfWork.RecommendationRequests.AddRequest(recommendationRequest);
            await this.unitOfWork.SaveChangesAsync(cancellationToken);

            // Upload the image to blob storage and get the URL.
            var imageBlobUrl = await this.blobStorageService.UploadImageAsync(image, recommendationRequest.CorrelationId, cancellationToken);            

            var recommendationsRequestedEvent = new RecommendationsRequestedEvent(imageBlobUrl);
            await this.eventPublisher.PublishAsync(recommendationsRequestedEvent, recommendationRequest.CorrelationId);
            return new RecommendationRequestDto { CorrelationId = recommendationRequest.CorrelationId };
        }

        /// <summary>
        /// Processes the recommendation result message.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="result">The recommendation result message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the processing operation.</returns>
        public async Task ProcessRecommendationsResultAsync(Guid correlationId, RecommendationResultMessageDto result, CancellationToken cancellationToken)
        {
            var recommendationRequest = await this.unitOfWork.RecommendationRequests.GetRequestAsync(correlationId, true, cancellationToken);
            if (recommendationRequest != null && recommendationRequest.Status == RecommendationRequestStatus.Pending)
            {
                if (result.Error == null)
                {
                    try
                    {
                        recommendationRequest.ExtractedIsbnsJson = JsonSerializer.Serialize(result.ExtractedBooks);
                        recommendationRequest.ResultJson = JsonSerializer.Serialize(result.RecommendedBooks);
                        recommendationRequest.Status = RecommendationRequestStatus.Completed;
                    }
                    catch (JsonException ex)
                    {
                        recommendationRequest.Error = ex.Message;
                        recommendationRequest.Status = RecommendationRequestStatus.Failed;
                    }
                }
                else
                {
                    recommendationRequest.Error = result.Error;
                    recommendationRequest.Status = RecommendationRequestStatus.Failed;
                }

                await this.unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Retrieves the recommendations for the given correlation id.
        /// </summary>
        /// <param name="correlationId">The correlation id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The recommendation result.</returns>
        public async Task<RecommendationResultDto> RetrieveRecommendationsAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            var recommendationRequest = await this.unitOfWork.RecommendationRequests.GetRequestAsync(correlationId, false, cancellationToken);
            if (recommendationRequest == null)
            {
                throw new RecommendationRequestNotFoundException(correlationId);
            }

            if (recommendationRequest.Status == RecommendationRequestStatus.Failed)
            {
                throw new RecommendationRequestFailedException(recommendationRequest.CorrelationId, recommendationRequest.Error!);
            }

            if (recommendationRequest.Status == RecommendationRequestStatus.Pending)
            {
                throw new RecommendationRequestPendingException(recommendationRequest.CorrelationId);
            }

            try
            {
                var extractedBooks = JsonSerializer.Deserialize<List<BookMessageDto>>(recommendationRequest.ExtractedIsbnsJson!);
                var recommendedBooksResult = JsonSerializer.Deserialize<List<BookMessageDto>>(recommendationRequest.ResultJson!);
                var recommendedIsbns = recommendedBooksResult?.Select(b => b.Isbn).ToHashSet() ?? [];
                var recommendedBooks = await this.unitOfWork.Books.GetByIsbnsAsync(recommendedIsbns, cancellationToken);
                var recommendedBookDtos = recommendedBooks.Select(BookModelDtoMapper.ToDto).ToList();
                return new RecommendationResultDto
                {
                    CorrelationId = recommendationRequest.CorrelationId,
                    ExtractedBooks = extractedBooks ?? [],
                    RecommendedBooks = recommendedBookDtos,
                    Error = null
                };
            }
            catch (JsonException ex)
            {
                throw new RecommendationRequestFailedException(recommendationRequest.CorrelationId, ex.Message);
            }
        }

        private static async Task ValidateImage(IFormFile image)
        {
            if (image == null)
            {
                throw new InvalidImageException("File is not provided");
            }

            if (image.Length == 0)
            {
                throw new InvalidImageException("File is empty");
            }

            if (image.Length > MaxImageSize)
            {
                throw new InvalidImageException($"File size exceeds the maximum limit of {MaxImageSize / (1024 * 1024)} MB");
            }

            if (!AllowedImageFormats.Contains(Path.GetExtension(image.FileName).ToLower()))
            {
                throw new InvalidImageException("Invalid file format. Only JPEG, JPG, and PNG are allowed.");
            }

            var imageInfo = await Image.IdentifyAsync(image.OpenReadStream());
            if (imageInfo == null
                || imageInfo.Width == 0
                || imageInfo.Height == 0
                || imageInfo.Metadata == null
                || imageInfo.Metadata.DecodedImageFormat == null
                || !AllowedImageFormats.Contains($".{imageInfo.Metadata.DecodedImageFormat.Name.ToLower()}"))
            {
                throw new InvalidImageException("Invalid image file");
            }
        }
    }
}