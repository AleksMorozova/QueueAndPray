using QueueAndPray.Abstractions.Jobs.Abstractions;

namespace QueueAndPray.Api.Workers.Messaging;

public sealed class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox publisher worker started. The messages are preparing for their tiny journey.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();

                await processor.ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Application is stopping.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher worker failed. The outbox tried to be reliable, but reality filed a complaint.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }

        _logger.LogInformation("Outbox publisher worker stopped. The messages will continue their existential crisis later.");
    }
}