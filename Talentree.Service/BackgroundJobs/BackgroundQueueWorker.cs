using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Talentree.Service.BackgroundJobs
{
    public class BackgroundQueueWorker : BackgroundService
    {
        private readonly IBackgroundJobQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundQueueWorker> _logger;

        public BackgroundQueueWorker(
            IBackgroundJobQueue queue,
            IServiceProvider serviceProvider,
            ILogger<BackgroundQueueWorker> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Queue Worker is starting.");

            // Run 5 concurrent processing loops to process items from the channel
            const int maxConcurrency = 5;
            var workers = new Task[maxConcurrency];

            for (int i = 0; i < maxConcurrency; i++)
            {
                workers[i] = ProcessQueueAsync(stoppingToken);
            }

            await Task.WhenAll(workers);

            _logger.LogInformation("Background Queue Worker is stopping.");
        }

        private async Task ProcessQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _queue.DequeueAsync(stoppingToken);

                    // Create a separate service scope for each enqueued work item
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        try
                        {
                            await workItem(scope.ServiceProvider, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred executing background job.");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Clean shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading from background queue.");
                }
            }
        }
    }
}
