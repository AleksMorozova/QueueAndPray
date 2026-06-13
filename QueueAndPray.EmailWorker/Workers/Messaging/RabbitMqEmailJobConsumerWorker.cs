using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.EmailWorker.Processing;

namespace QueueAndPray.EmailWorker.Workers.Messaging;

public sealed class RabbitMqEmailJobConsumerWorker : BackgroundService
{
    private readonly RabbitMqJobQueueConsumer _consumer;
    private readonly ILogger<RabbitMqEmailJobConsumerWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqEmailJobConsumerWorker(
        RabbitMqJobQueueConsumer consumer,
        ILogger<RabbitMqEmailJobConsumerWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _consumer = consumer;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.RunAsync(
            JobType.Email,
            "email",
            _scopeFactory,
            async (provider, message, ct) =>
            {
                var handler = provider.GetRequiredService<IEmailJobQueuedMessageHandler>();
                await handler.HandleAsync(message, ct);
            },
            _logger,
            stoppingToken);
    }
}
