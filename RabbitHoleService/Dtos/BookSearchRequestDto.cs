using Microsoft.AspNetCore.Mvc;
using RabbitHoleService.ModelBinders;
using RabbitHoleService.Objects;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Dtos
{
    /// <summary>
    /// The book search request dto.
    /// </summary>
    public class BookSearchRequestDto
    {
        private string? name;
        private string? author;
        private string? isbn;

        /// <summary>
        /// Gets the ISBN.
        /// </summary>
        [StringLength(13, ErrorMessage = "The ISBN should not exceed 13 characters.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "The ISBN must contain only numbers.")]
        public string? Isbn
        {
            get => this.isbn;
            init => this.isbn = value?.Trim();
        }

        /// <summary>
        /// Gets the name.
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
        /// Gets the minimum cost.
        /// </summary>
        [Range(0.01, 10000, ErrorMessage = "The cost must be between 0.01 and 10,000.")]
        public decimal? MinimumCost { get; init; }

        /// <summary>
        /// Gets the maximum cost.
        /// </summary>
        [Range(0.01, 10000, ErrorMessage = "The cost must be between 0.01 and 10,000.")]
        public decimal? MaximumCost { get; init; }

        /// <summary>
        /// Gets the genres.
        /// </summary>
        [ModelBinder(BinderType = typeof(GenreListModelBinder))]
        public HashSet<GenreType>? Genres { get; init; }
    }
}
