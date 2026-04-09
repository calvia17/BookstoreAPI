using RabbitHoleService.Objects;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The Create Book dto.
    /// </summary>
    public class CreateBookDto
    {
        private string name = string.Empty;
        private string author = string.Empty;
        private string isbn = string.Empty;

        /// <summary>
        /// Get the name.
        /// </summary>
        [Required(ErrorMessage = "The book title is required.")]
        [StringLength(250, ErrorMessage = "The book name should not exceed 250 characters.")]
        public required string Name
        { 
            get => this.name;
            init => this.name = value.Trim();
        }

        /// <summary>
        /// Get the isbn.
        /// </summary>
        [Required(ErrorMessage = "The book ISBN is required.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "The ISBN must contain only numbers.")]
        [StringLength(13, ErrorMessage = "The book ISBN should not exceed 13 characters.")]
        public required string Isbn
        {
            get => this.isbn;
            init => this.isbn = value.Trim();
        }

        /// <summary>
        /// Gets the author.
        /// </summary>
        [Required(ErrorMessage = "The author name is required.")]
        [StringLength(200, ErrorMessage = "The author name should not exceed 200 characters.")]
        public required string Author
        {
            get => this.author;
            init => this.author = value.Trim();
        }

        /// <summary>
        /// Gets the cost.
        /// </summary>
        [Required(ErrorMessage = "The cost is required.")]
        [Range(0.01, 10000, ErrorMessage = "The cost must be between 0.01 and 10,000.")]
        public decimal? Cost { get; init; }

        /// <summary>
        /// Gets the stock.
        /// </summary>
        [Required(ErrorMessage = "The stock is required.")]
        [Range(0, 10000, ErrorMessage = "The stock must be between 0 and 10,000.")]
        public int? Stock { get; init; }

        /// <summary>
        /// Gets the genres.
        /// </summary>
        [Required(ErrorMessage = "The genres are required.")]
        [MinLength(1, ErrorMessage = "The book should contain atleast one genre.")]
        public required HashSet<GenreType> GenreIds { get; init; } = new();
    }
}
