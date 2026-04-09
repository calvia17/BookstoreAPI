using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The genre class.
    /// </summary>
    public class Genre
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public GenreType Id { get; init; } // The id is the GenreType enum

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// The book orders.
        /// </summary>
        public ICollection<BookGenre> BookGenres { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="Genre" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        public Genre(GenreType id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.BookGenres = new List<BookGenre>();
        }
    }
}
