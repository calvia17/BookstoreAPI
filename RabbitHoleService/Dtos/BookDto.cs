namespace RabbitHoleService.Dtos
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The book dto.
    /// </summary>
    public class BookDto
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Get the name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Get or sets the isbn.
        /// </summary>
        public required string Isbn { get; init; }

        /// <summary>
        /// Gets the author.
        /// </summary>
        public required string Author { get; init; }

        /// <summary>
        /// Gets the cost.
        /// </summary>
        public decimal Cost { get; init; }

        /// <summary>
        /// Gets the stock.
        /// </summary>
        public int Stock { get; init; }

        /// <summary>
        /// Gets the genres.
        /// </summary>
        public required HashSet<GenreDto> Genres { get; init; } = new();

        /// <summary>
        /// Gets the last modified date.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset LastModified { get; set; }
    }
}
