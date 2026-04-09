using System.ComponentModel.DataAnnotations;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The book genre class.
    /// </summary>
    public class BookGenre
    {
        /// <summary>
        /// The book id.
        /// </summary>
        [Required]
        public Guid BookId { get; init; }

        /// <summary>
        /// The genre id.
        /// </summary>
        [Required]
        public GenreType GenreId { get; init; }

        /// <summary>
        /// The book.
        /// </summary>
        public Book Book { get; set; } = null!;

        /// <summary>
        /// The genre.
        /// </summary>
        public Genre Genre { get; set; } = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookGenre" /> class.
        /// </summary>
        /// <param name="bookId">The book id.</param>
        /// <param name="genreId">The genre id.</param>
        public BookGenre(Guid bookId, GenreType genreId)
        {
            this.BookId = bookId;
            this.GenreId = genreId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BookGenre" /> class.
        /// </summary>
        /// <param name="book">The book.</param>
        /// <param name="genreId">The genre id.</param>
        public BookGenre(Book book, GenreType genreId)
        {
            this.Book = book;
            this.GenreId = genreId;
        }
    }
}
