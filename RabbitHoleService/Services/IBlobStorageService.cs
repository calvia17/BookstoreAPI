namespace RabbitHoleService.Services
{
    /// <summary>
    /// The blob storage service interface.
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads an image to blob storage.
        /// </summary>
        /// <param name="image">The image to upload.</param>
        /// <param name="correlationId">The correlation ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The URL of the uploaded image.</returns>
        Task<string> UploadImageAsync(IFormFile image, Guid correlationId, CancellationToken cancellationToken = default);
    }
}
