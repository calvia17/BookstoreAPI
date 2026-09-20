using RabbitHoleService.Events;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The book event publisher interface.
    /// </summary>
    public interface IBookEventPublisher
    {
        /// <summary>
        /// Publishes the message to the service bus.
        /// </summary>
        /// <param name="bookEvent">The book event to publish.</param>
        /// <param name="correlationId">The correlation id.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PublishAsync(BookEvent bookEvent, Guid? correlationId = null);
    }
}
