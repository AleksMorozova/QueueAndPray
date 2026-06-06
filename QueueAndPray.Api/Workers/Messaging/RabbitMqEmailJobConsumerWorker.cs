using Microsoft.Extensions.Options;
using QueueAndPray.Application.Common.Messaging;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using System.Text;

namespace QueueAndPray.Api.Workers.Messaging;

public class RabbitMqEmailJobConsumerWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqEmailJobConsumerWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqEmailJobConsumerWorker(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqEmailJobConsumerWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var deadLetterQueue = $"{MessagingTopology.JobQueue(JobType.Email)}.dead-letter";

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = "",
            ["x-dead-letter-routing-key"] = deadLetterQueue
        };

        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: MessagingTopology.JobQueue(JobType.Email),
            type: RabbitMQ.Client.ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: MessagingTopology.JobQueue(JobType.Email),
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: MessagingTopology.JobQueue(JobType.Email),
            exchange: MessagingTopology.EventsExchangeName,
            routingKey: RoutingKeyHelper.Queued(JobType.Email),
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await channel.BasicGetAsync(
                queue: MessagingTopology.JobQueue(JobType.Email),
                autoAck: false,
                cancellationToken: stoppingToken);

            if (result is null)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            try
            {
                var message = Encoding.UTF8.GetString(result.Body.ToArray());

                _logger.LogInformation(
                    "RabbitMQ email job message received: {Message}",
                    message);

                await using var scope = _scopeFactory.CreateAsyncScope();

                var handler = scope.ServiceProvider
                    .GetRequiredService<IJobQueuedMessageHandler>();

                await handler.HandleAsync(message, stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: result.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process RabbitMQ status message"); 

                await channel.BasicNackAsync(
                    deliveryTag: result.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        }
    }
}
