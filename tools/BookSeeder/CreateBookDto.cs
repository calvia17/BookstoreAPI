namespace BookSeeder
{
    /// <summary>
    /// The create book dto.
    /// </summary>
    public class CreateBookDto
    {
        /// <summary>
        /// The name of the book.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The author.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The isbn.
        /// </summary>
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// The cost.
        /// </summary>
        public decimal Cost { get; set; } = 20;

        /// <summary>
        /// The stock.
        /// </summary>
        public int Stock { get; set; } = 10;

        /// <summary>
        /// The genre ids.
        /// </summary>
        public HashSet<GenreType> GenreIds { get; set; } = new();
    }
}
