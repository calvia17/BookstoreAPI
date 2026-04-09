using RabbitHoleService.Objects;

namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The duplicate item record.
    /// </summary>
    /// <param name="Id">The item id.</param>
    /// <param name="Isbn">The item ISBN.</param>
    /// <param name="Name">The item name.</param>
    public record DuplicateItem(Guid Id, string Isbn, string Name);

    /// <summary>
    /// The duplicate item exception.
    /// </summary>
    public class DuplicateItemException : BaseApplicationException
    {
        /// <summary>
        /// Gets the duplicate items.
        /// </summary>
        public IEnumerable<DuplicateItem> DuplicateItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateItemException" /> class.
        /// </summary>
        /// <param name="books">The books.</param>
        public DuplicateItemException(IEnumerable<DuplicateItem> books)
            : base($"Duplicate items have been added to the order.")
        {
            this.DuplicateItems = books.Select(b => new DuplicateItem(b.Id, b.Isbn, b.Name)).ToList();
        }
    }
}
