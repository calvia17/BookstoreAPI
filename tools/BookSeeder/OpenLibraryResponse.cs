using System.Text.Json.Serialization;

namespace BookSeeder
{
    /// <summary>
    /// The open library response.
    /// </summary>
    public class OpenLibraryResponse
    {
        /// <summary>
        /// The books.
        /// </summary>
        [JsonPropertyName("docs")]
        public List<OpenLibraryBook>? Docs { get; set; }
    }
}
