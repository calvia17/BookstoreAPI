using RabbitHoleService.Dtos;
using RabbitHoleService.Objects;

namespace RabbitHoleService.Mappers
{
    /// <summary>
    /// The book model dto mapper.
    /// </summary>
    public class BookModelDtoMapper
    {
        /// <summary>
        /// Maps a book to its dto.
        /// </summary>
        /// <param name="book">The book model.</param>
        /// <returns>The dto.</returns>
        public static BookDto ToDto(Book book)
        {
            ArgumentNullException.ThrowIfNull(book);

            return new BookDto()
            {
                Id = book.Id,
                Name = book.Name,
                Isbn = book.Isbn,
                Author = book.Author,
                Cost = book.Cost,
                Stock = book.Stock,
                Genres = book.BookGenres.Select(bg => new GenreDto
                {
                    Id = bg.GenreId,
                    Name = ((GenreType)bg.GenreId).ToString()
                }).ToHashSet(),
            };
        }

        /// <summary>
        /// Converts the dto into a book model.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns>The model.</returns>
        public static Book ToModel(CreateBookDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var book = new Book(dto.Name, dto.Isbn, dto.Author, dto.Cost!.Value, dto.Stock!.Value);
            foreach (var genreId in dto.GenreIds)
            {
                // When creating a new book, we use the navigation property to link the book and genre together since the book's ID doesn't exist yet.
                book.BookGenres.Add(new BookGenre(book, genreId));
            }

            return book;
        }

        /// <summary>
        /// Converts the dto into a book model.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns>The model.</returns>
        public static Book ToModel(BookDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var book = new Book(dto.Id, dto.Name, dto.Isbn, dto.Author, dto.Cost, dto.Stock);
            foreach (var genre in dto.Genres)
            {
                book.BookGenres.Add(new BookGenre(book.Id, genre.Id));
            }

            return book;
        }
    }
}