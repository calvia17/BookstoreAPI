using RabbitHoleService.Dtos;
using RabbitHoleService.Services;
using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The book class.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        [Key]
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [Required(ErrorMessage = "The book name is required.")]
        [StringLength(250)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the isbn.
        /// </summary>
        [Required(ErrorMessage = "The book ISBN is required.")]
        [StringLength(13, MinimumLength = 13)]
        public string Isbn { get; private set; }

        /// <summary>
        /// Gets the author.
        /// </summary>
        [Required(ErrorMessage = "The author name is required.")]
        [StringLength(200)]
        public string Author { get; private set; }

        /// <summary>
        /// Gets the cost.
        /// </summary>
        [Range(0.01, 10000, ErrorMessage = "The cost must be between 0.01 and 10,000.")]
        public decimal Cost { get; private set; }

        /// <summary>
        /// Gets the stock.
        /// </summary>
        [Range(0, 10000, ErrorMessage = "The stock must be between 0 and 10,000.")]
        public int Stock { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the book is deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the last modified date.
        /// </summary>
        public DateTimeOffset LastModified { get; private set; }

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
        /// <param name="author">The author.</param>rd
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
            this.LastModified = DateTimeOffset.UtcNow;
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

        /// <summary>
        /// Updates the book properties.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="author">The author.</param>
        /// <param name="cost">The cost.</param>
        /// <param name="stock">The stock.</param>
        /// <param name="genreIds">The genre IDs.</param>
        public void UpdateBookProperties(string? name, string? author, decimal? cost, int? stock, HashSet<GenreType>? genreIds)
        {
            var hasChanged = false;
            if (!string.IsNullOrEmpty(name))
            {
                this.Name = name;
                hasChanged = true;
            }

            if (!string.IsNullOrEmpty(author))
            {
                this.Author = author;
                hasChanged = true;
            }

            if (cost.HasValue)
            {
                this.Cost = cost.Value;
                hasChanged = true;
            }

            if (stock.HasValue)
            {
                this.Stock = stock.Value;
                hasChanged = true;
            }

            if (genreIds != null)
            {
                this.SyncGenres(genreIds);
                hasChanged = true;
            }

            if (hasChanged)
            {
                this.LastModified = DateTimeOffset.UtcNow;
            }
        }

        private void SyncGenres(HashSet<GenreType> newGenreIds)
        {
            var genresToRemove = this.BookGenres.Where(bg => !newGenreIds.Contains(bg.GenreId)).ToList();
            var existingGenreIds = this.BookGenres.Select(bg => bg.GenreId).ToHashSet();
            var genresToAdd = newGenreIds.Where(g => !existingGenreIds.Contains(g)).Select(g => new BookGenre(this.Id, g)).ToList();
            genresToRemove.ForEach(bg => this.BookGenres.Remove(bg));
            genresToAdd.ForEach(bg => this.BookGenres.Add(bg));
        }

        /// <summary>
        /// Updates the stock.
        /// </summary>
        /// <param name="newStock">The new stock.</param>
        public void UpdateStock(int newStock)
        {
            this.Stock = newStock;
            this.LastModified = DateTimeOffset.UtcNow;
        }

        public void DeleteBook()
        {
            this.IsDeleted = true;
            this.LastModified = DateTimeOffset.UtcNow;
        }
    }
}
