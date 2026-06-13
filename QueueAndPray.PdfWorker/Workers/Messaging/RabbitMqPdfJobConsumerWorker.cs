using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.PdfWorker.Processing;

namespace QueueAndPray.PdfWorker.Workers.Messaging;

public sealed class RabbitMqPdfJobConsumerWorker : BackgroundService
{
    private readonly RabbitMqJobQueueConsumer _consumer;
    private readonly ILogger<RabbitMqPdfJobConsumerWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqPdfJobConsumerWorker(
        RabbitMqJobQueueConsumer consumer,
        ILogger<RabbitMqPdfJobConsumerWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _consumer = consumer;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.RunAsync(
            JobType.PdfGeneration,
            "PDF",
            _scopeFactory,
            async (provider, message, ct) =>
            {
                var handler = provider.GetRequiredService<PdfJobQueuedMessageHandler>();
                await handler.HandleAsync(message, ct);
            },
            _logger,
            stoppingToken);
    }
}
