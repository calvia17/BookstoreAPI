using System.Text.Json.Serialization;

namespace BookSeeder
{
    /// <summary>
    /// The open library book.
    /// </summary>
    public class OpenLibraryBook
    {
        /// <summary>
        /// The title.
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// The authors.
        /// </summary>
        [JsonPropertyName("author_name")]
        public List<string>? Authors { get; set; }

        /// <summary>
        /// The isbns.
        /// </summary>
        [JsonPropertyName("isbn")]
        public List<string>? Isbns { get; set; }

        /// <summary>
        /// The genre.
        /// </summary>
        public GenreType Genre { get; set; }
    }
}
