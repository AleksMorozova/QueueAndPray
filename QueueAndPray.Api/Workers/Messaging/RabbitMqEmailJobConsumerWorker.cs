using Microsoft.Extensions.Options;
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
   private readonly IEmailJobProcessor _processor;

    public RabbitMqEmailJobConsumerWorker(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        IEmailJobProcessor processor,
        ILogger<RabbitMqEmailJobConsumerWorker> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _processor = processor;
        _logger = logger;
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

                var envelope = JsonSerializer.Deserialize<JobQueuedEventEnvelope>(message);

                if (envelope is null)
                {
                    throw new InvalidOperationException(
                        "RabbitMQ status envelope is empty. QueueAndPray is confused.");
                }

                _logger.LogInformation("Email job received: {Message}", message);

                var payload = JsonSerializer.Deserialize<JobQueuedEvent>(envelope.Payload);

                await ProcessWithRetryAsync(payload, stoppingToken);

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

    private async Task ProcessWithRetryAsync(
        JobQueuedEvent payload,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await _processor.SendEmailAsync(payload, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(
                    ex,
                    "Email job {JobId} failed on attempt {Attempt}. Retrying...",
                    payload.JobId,
                    attempt);

                await Task.Delay(
                    TimeSpan.FromSeconds(attempt),
                    cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Email job '{payload.JobId}' failed after {maxAttempts} attempts.");
    }
}
