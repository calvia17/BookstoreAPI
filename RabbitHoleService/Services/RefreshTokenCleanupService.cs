
using RabbitHoleService.Repositories;

namespace RabbitHoleService.Services
{
    /// <summary>
    /// The refresh token cleanup service.
    /// </summary>
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenCleanupService" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public RefreshTokenCleanupService(IServiceProvider serviceProvider) 
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
            await this.DeleteExpiredTokensAsync(stoppingToken); // Run immediately on startup
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await this.DeleteExpiredTokensAsync(stoppingToken);
            }
        }

        private async Task DeleteExpiredTokensAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Create a scope to resolve IRefreshTokenRepository.
                // This is a singleton service, so scoped dependencies cannot be injected directly through the constructor.
                using var scope = this.serviceProvider.CreateScope();
                var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                await refreshTokenRepository.DeleteExpiredTokensAsync(DateTimeOffset.UtcNow, stoppingToken);
            }
            catch
            {
                // Suppress exceptions to keep the background service running.
            }
        }
    }
}
