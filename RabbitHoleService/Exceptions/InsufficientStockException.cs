namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The insufficient stock record.
    /// </summary>
    /// <param name="BookId">The book id.</param>
    /// <param name="Isbn">The isbn.</param>
    /// <param name="Name">The book name.</param>
    /// <param name="AvailableStock">The available stock.</param>
    public record InsufficientStockItem(Guid BookId, string Isbn, string Name, int AvailableStock);

    /// <summary>
    /// The insufficient stock exception.
    /// </summary>
    public class InsufficientStockException : BaseApplicationException
    {
        /// <summary>
        /// Gets the insufficient stock items.
        /// </summary>
        public IEnumerable<InsufficientStockItem> InsufficientStockItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InsufficientStockException" /> class.
        /// </summary>
        /// <param name="insufficentStockItems">The items with insufficient stock.</param>
        public InsufficientStockException(IEnumerable<InsufficientStockItem> insufficentStockItems)
            : base($"Insufficient stock to complete the order.")
        {
            this.InsufficientStockItems = insufficentStockItems;
        }
    }
}
