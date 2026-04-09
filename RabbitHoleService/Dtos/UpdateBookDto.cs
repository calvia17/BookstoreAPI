using RabbitHoleService.Objects;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The update book dto.
    /// </summary>
    public class UpdateBookDto
    {
        private string? name;
        private string? author;

        /// <summary>
        /// Get the name.
        /// </summary>
        [StringLength(250, ErrorMessage = "The book name should not exceed 250 characters.")]
        public string? Name
        { 
            get => this.name;
            init => this.name = value?.Trim();
        }

        /// <summary>
        /// Gets the author.
        /// </summary>
        [StringLength(200, ErrorMessage = "The author name should not exceed 200 characters.")]
        public string? Author
        { 
            get => this.author;
            init => this.author = value?.Trim();
        }

        /// <summary>
        /// Gets the cost.
        /// </summary>
        [Range(0.01, 10000, ErrorMessage = "The cost must be between 0.01 and 10,000.")]
        public decimal? Cost { get; init; }

        /// <summary>
        /// Gets the stock.
        /// </summary>
        [Range(0, 10000, ErrorMessage = "The stock must be between 0 and 10,000.")]
        public int? Stock { get; init; }

        /// <summary>
        /// Gets the genres.
        /// </summary>
        [MinLength(1, ErrorMessage = "The book should contain atleast one genre.")]
        public HashSet<GenreType>? GenreIds { get; init; }
    }
}