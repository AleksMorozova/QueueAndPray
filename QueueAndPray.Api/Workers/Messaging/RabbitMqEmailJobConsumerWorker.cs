using Microsoft.Extensions.Options;
using QueueAndPray.Application.Common.Resilience;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
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

        await channel.QueueDeclareAsync(
            queue: _options.EmailJobQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            Guid jobId = Guid.Empty;

            var result = await channel.BasicGetAsync(
                queue: _options.EmailJobQueueName,
                autoAck: false,
                cancellationToken: stoppingToken);

            if (result is null)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            var failureTracker = scope.ServiceProvider.GetRequiredService<IJobFailureTracker>();

            try
            {
                var message = Encoding.UTF8.GetString(result.Body.ToArray());

                _logger.LogInformation(
                    "RabbitMQ status message received: {Message}",
                    message);

                var envelope = JsonSerializer.Deserialize<JobQueuedEventEnvelope>(message);

                if (envelope is null)
                {
                    throw new InvalidOperationException(
                        "RabbitMQ status envelope is empty. QueueAndPray is confused.");
                }

                _logger.LogInformation("Email job received: {Message}", message);

                var payload = JsonSerializer.Deserialize<JobQueuedEvent>(envelope.Payload);
                jobId = payload?.JobId ?? throw new InvalidOperationException(
                    "RabbitMQ status payload is empty. QueueAndPray is confused.");

                var emailJobOrchestrator = scope.ServiceProvider.GetRequiredService<IEmailJobOrchestrator>();

                await emailJobOrchestrator.ProcessAsync(payload, stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: result.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process RabbitMQ status message"); 

                await failureTracker.TrackDeadLetterAsync(jobId, ex.Message, stoppingToken);

                await channel.BasicNackAsync(
                    deliveryTag: result.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        }
    }
}
