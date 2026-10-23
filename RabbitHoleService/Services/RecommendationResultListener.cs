using Azure.Messaging.ServiceBus;
using RabbitHoleService.Dtos.Recommendations;
using System.Text.Json;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The recommendation result listener.
    /// </summary>
    public class RecommendationResultListener : BackgroundService
    {
        private readonly ServiceBusProcessor processor;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes the recommendation result listener.
        /// </summary>
        /// <param name="processor">The service bus processor.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public RecommendationResultListener(ServiceBusProcessor processor, IServiceProvider serviceProvider)
        {
            this.processor = processor;
            this.processor.ProcessMessageAsync += HandleMessageAsync;
            this.processor.ProcessErrorAsync += HandleErrorAsync;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Starts processing messages from the queue.
            await this.processor.StartProcessingAsync(stoppingToken);

            // Waits until the service is stopped.
            // This keeps the worker running and processing messages until the application is stopped.
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessageAsync(ProcessMessageEventArgs args)
        {
            // This processes each message received from the queue.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                using var scope = this.serviceProvider.CreateScope();
                var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
                var result = args.Message.Body.ToObjectFromJson<RecommendationResultMessageDto>(options);
                if (result == null)
                {
                    // Add to dead letter queue so it can be investigated later.
                    await args.DeadLetterMessageAsync(args.Message, "DeserializationError", "Message body is null after deserialization.", args.CancellationToken);
                    return;
                }

                if (Guid.TryParse(args.Message.CorrelationId, out var correlationId))
                {
                    await recommendationService.ProcessRecommendationsResultAsync(correlationId, result, args.CancellationToken);

                    // Processing completed successfully, so remove the message from the queue.
                    await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                }
                else
                {
                    await args.DeadLetterMessageAsync(args.Message, "InvalidCorrelationId", "Message correlation id is invalid.", args.CancellationToken);
                }
            } 
            catch (JsonException ex) 
            {
                // Add to dead letter queue so it can be investigated later.
                await args.DeadLetterMessageAsync(args.Message, "DeserializationError", ex.Message, args.CancellationToken);
                return;
            }
        }

        private Task HandleErrorAsync(ProcessErrorEventArgs args)
        {
            // Log error here later.
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // The DI container will dispose processor, sender, and client.
            await this.processor.StopProcessingAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
