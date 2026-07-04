using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The book class.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public Guid Id { get; init; }

        /// <summary>
        /// Get or sets the name.
        /// </summary>
        [Required(ErrorMessage = "The book name is required.")]
        [StringLength(250)]
        public string Name { get; set; }

        /// <summary>
        /// Get or sets the isbn.
        /// </summary>
        [Required(ErrorMessage = "The book ISBN is required.")]
        [StringLength(13, MinimumLength = 13)]
        public string Isbn { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        [Required(ErrorMessage = "The author name is required.")]
        [StringLength(200)]
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        [Range(0.01, 10000, ErrorMessage = "The cost must be between 0.01 and 10,000.")]
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the stock.
        /// </summary>
        [Range(0, 10000, ErrorMessage = "The stock must be between 0 and 10,000.")]
        public int Stock { get; set; }

        /// <summary>
        /// A value indicating whether the book is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets the book orders.
        /// </summary>
        public ICollection<BookOrder> BookOrders { get; } = null!;

        /// <summary>
        /// Gets the book genres.
        /// </summary>
        public ICollection<BookGenre> BookGenres { get; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="Book" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="isbn">The isbn.</param>
        /// <param name="author">The author.</param>
        /// <param name="cost">The cost.</param>
        /// <param name="stock">The stock.</param>
        public Book(string name, string isbn, string author, decimal cost, int stock)
        {
            this.Name = name;
            this.Isbn = isbn;
            this.Author = author;
            this.Cost = cost;
            this.Stock = stock;
            this.BookOrders = new HashSet<BookOrder>();
            this.BookGenres = new HashSet<BookGenre>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Book" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="name">The name.</param>
        /// <param name="isbn">The isbn.</param>
        /// <param name="author">The author.</param>
        /// <param name="cost">The cost.</param>
        /// <param name="stock">The stock.</param>
        public Book(Guid id, string name, string isbn, string author, decimal cost, int stock)
            : this(name, isbn, author, cost, stock)
        {
            this.Id = id;
        }
    }
}
