using Azure.Messaging.ServiceBus;
using RabbitHoleService.Events;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The book event publisher.
    /// </summary>
    public class BookEventPublisher : IBookEventPublisher
    {
        private readonly ServiceBusSender sender;

        /// <summary>
        /// Initializes the book event publisher.
        /// </summary>
        /// <param name="sender">The Service Bus sender.</param>
        public BookEventPublisher(ServiceBusSender sender)
        {
            this.sender = sender;
        }

        /// <summary>
        /// Publishes the message to the service bus.
        /// </summary>
        /// <param name="bookEvent">The book event to publish.</param>
        /// <param name="correlationId">The correlation id.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task PublishAsync(BookEvent bookEvent, Guid? correlationId = null)
        {
            var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(bookEvent)) { CorrelationId = correlationId.ToString() };
            await sender.SendMessageAsync(message);
        }
    }
}
