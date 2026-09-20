namespace RabbitHoleService.Exceptions.Recommendations
{
    /// <summary>
    /// The image upload failed exception which is thrown when the image upload to blob storage fails.
    /// </summary>
    public class ImageUploadFailedException : BaseApplicationException
    {
        /// <summary>
        /// Gets the error.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageUploadFailedException" /> class.
        /// </summary>
        /// <param name="error">The error.</param>
        public ImageUploadFailedException(string error)
            : base("Failed to upload image to blob storage.")
        {
            this.Error = error;
        }
    }
}
