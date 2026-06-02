using Microsoft.Extensions.Options;
using QueueAndPray.Application.Common.Resilience;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Application.Jobs.Messaging;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using System.Text;
using System.Text.Json;

namespace QueueAndPray.Api.Workers.Messaging;

public class RabbitMqEmailJobConsumerWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEmailJobConsumerWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqEmailJobConsumerWorker(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEmailJobConsumerWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var deadLetterQueue = $"{_options.EmailJobQueueName}.dead-letter";
     
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
            exchange: _options.EventsExchangeName,
            type: RabbitMQ.Client.ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: _options.EmailJobQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: _options.EmailJobQueueName,
            exchange: _options.EventsExchangeName,
            routingKey: RoutingKeys.JobQueued,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await channel.BasicGetAsync(
                queue: _options.EmailJobQueueName,
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
                    "RabbitMQ status message received: {Message}",
                    message);

                var envelope = JsonSerializer.Deserialize<JobQueuedEvent>(message);

                if (envelope is null)
                {
                    throw new InvalidOperationException(
                        "RabbitMQ status envelope is empty. QueueAndPray is confused.");
                }

                _logger.LogInformation("Email job received: {Message}", message);

                await using var scope = _scopeFactory.CreateAsyncScope();

                var emailJobOrchestrator = scope.ServiceProvider.GetRequiredService<IEmailJobOrchestrator>();

                await emailJobOrchestrator.ProcessAsync(envelope, stoppingToken);

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
