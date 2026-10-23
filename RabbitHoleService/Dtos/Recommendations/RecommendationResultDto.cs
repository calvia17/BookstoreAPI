using RabbitHoleService.Dtos.Books;

namespace RabbitHoleService.Dtos.Recommendations
{
    /// <summary>
    /// The recommendation result dto.
    /// </summary>
    public class RecommendationResultDto
    {
        /// <summary>
        /// Gets the correlation id.
        /// </summary>
        public Guid CorrelationId { get; init; }

        /// <summary>
        /// Gets the extracted books.
        /// </summary>
        public required List<ExtractedBookDto> ExtractedBooks { get; init; }

        /// <summary>
        /// Gets the recommended books.
        /// </summary>
        public required List<BookDto> RecommendedBooks { get; init; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public string? Error { get; init; }
    }
}
