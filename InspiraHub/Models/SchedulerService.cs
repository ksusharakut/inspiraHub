using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InspiraHub.Models
{
    public class SchedulerService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private System.Threading.Timer _timerNotification;
        public IConfiguration _iconfiguration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;

        public SchedulerService(IServiceScopeFactory serviceScopeFactory, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IConfiguration iconfiguration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _env = env;
            _iconfiguration = iconfiguration;
        }
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timerNotification = new Timer(RunJob, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1)); //set interval time here
            return Task.CompletedTask;
        }
        public void RunJob(object state)
        {
            using (var scrope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    //place code here which you want to schedule on regular intervals
                }
                catch (Exception ex)
                {

                }
                Interlocked.Increment(ref executionCount);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timerNotification?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timerNotification?.Dispose();
        }
    }
}
