using RabbitHoleService.Repositories;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The recommendation request cleanup service.
    /// </summary>
    public class RecommendationRequestCleanupService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecommendationRequestCleanupService" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public RecommendationRequestCleanupService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the background service task.
        /// </summary>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>The task representing the background service task execution.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.DeleteExpiredRecommendationRequestsAsync(stoppingToken); // Run immediately on startup
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await this.DeleteExpiredRecommendationRequestsAsync(stoppingToken);
            }
        }

        private async Task DeleteExpiredRecommendationRequestsAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Create a scope to resolve IRefreshTokenRepository.
                // This is a singleton service, so scoped dependencies cannot be injected directly through the constructor.
                using var scope = this.serviceProvider.CreateScope();
                var recommendationRequestRepository = scope.ServiceProvider.GetRequiredService<IRecommendationRequestRepository>();
                await recommendationRequestRepository.DeleteExpiredRequestsAsync(DateTimeOffset.UtcNow.AddDays(-1), stoppingToken);
            }
            catch
            {
                // Suppress exceptions to keep the background service running.
            }
        }
    }
}
