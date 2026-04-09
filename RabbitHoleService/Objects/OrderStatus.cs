namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The order status enum.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Pending status.
        /// </summary>
        Pending,

        /// <summary>
        /// The processed status.
        /// </summary>
        Processed,

        /// <summary>
        /// Shipped status.
        /// </summary>
        Shipped,

        /// <summary>
        /// The delivered status.
        /// </summary>
        Delivered,

        /// <summary>
        /// Cancelled status.
        /// </summary>
        Cancelled
    }
}
