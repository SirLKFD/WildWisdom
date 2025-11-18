using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.HostedServices
{
    public class OverdueReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueReminderBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public OverdueReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OverdueReminderBackgroundService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _configuration.GetValue<int?>("EmailSettings:OverdueReminderIntervalMinutes") ?? 1440;
            if (intervalMinutes <= 0)
            {
                intervalMinutes = 1440;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var reminderService = scope.ServiceProvider.GetRequiredService<IOverdueReminderService>();
                    await reminderService.SendOverdueRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process overdue reminder emails.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // shutdown
                }
            }
        }
    }
}

