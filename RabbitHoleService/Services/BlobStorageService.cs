using Azure;
using Azure.Storage.Blobs;
using RabbitHoleService.Exceptions.Recommendations;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The blob storage service.
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient blobContainer;

        /// <summary>
        /// Initializes the blob storage service.
        /// </summary>
        /// <param name="blobContainer"></param>
        public BlobStorageService(BlobContainerClient blobContainer)
        {
            this.blobContainer = blobContainer;
        }

        /// <summary>
        /// Uploads an image to blob storage.
        /// </summary>
        /// <param name="image">The image to upload.</param>
        /// <param name="correlationId">The correlation ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The URL of the uploaded image.</returns>
        public async Task<string> UploadImageAsync(IFormFile image, Guid correlationId, CancellationToken cancellationToken = default)
        {
            var blobName = $"{correlationId}{Path.GetExtension(image.FileName)}"; 
            var blobClient = blobContainer.GetBlobClient(blobName);
            try
            {
                await using var stream = image.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true, cancellationToken); 
                return blobClient.Uri.ToString();
            }
            catch (Exception ex) when (ex is RequestFailedException or AggregateException)
            {
                throw new ImageUploadFailedException(ex.Message);
            }
        }
    }
}
