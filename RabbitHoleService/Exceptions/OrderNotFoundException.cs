namespace RabbitHoleService.Exceptions
{
    /// <summary>
    /// The order not found exception.
    /// </summary>
    public class OrderNotFoundException : BaseApplicationException
    {
        /// <summary>
        /// Gets the order id that was not found.
        /// </summary>
        public Guid OrderId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderNotFoundException" /> class.
        /// </summary>
        /// <param name="id">The id.</param>
        public OrderNotFoundException(Guid id)
            : base($"The requested order with ID '{id}' was not found.")
        {
            this.OrderId = id;
        }
    }
}
