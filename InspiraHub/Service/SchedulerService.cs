using InspiraHub.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InspiraHub.Service
{
    public class SchedulerService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private int executionCount;

        public SchedulerService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)); // Проверка каждые 10 секунд
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InspirahubContext>();
                var expirationTime = DateTime.Now.AddMinutes(-5);
                Console.WriteLine(expirationTime);
                var expiredTokens = context.PasswordResetTokens
                    .Where(t => t.CreatedAt < expirationTime)
                    .ToList();
                Console.WriteLine($"expired tokens: {expiredTokens}");

                if (expiredTokens.Any())
                {
                    try
                    {
                        context.PasswordResetTokens.RemoveRange(expiredTokens);
                        context.SaveChanges();
                        Console.WriteLine($"Removed {expiredTokens.Count} expired tokens.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error occurred while removing expired tokens: {ex.Message}");
                    }
                    // _logger.LogInformation($"Removed {expiredTokens.Count} expired tokens.");
                }
                Interlocked.Increment(ref executionCount);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
