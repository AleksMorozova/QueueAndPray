using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;

public sealed class RabbitMqJobQueue : IJobQueue
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqJobQueue> _logger;

    public RabbitMqJobQueue(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqJobQueue> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task PublishAsync(
        JobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        var queueName = Resolve(jobQueuedEvent.Type);

        var deadLetterQueue = $"{queueName}.dead-letter";

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = "",
            ["x-dead-letter-routing-key"] = deadLetterQueue
        };

        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var q =  channel.CurrentQueue;

        var envelope = new JobQueuedEventEnvelope
        {
            EventType = jobQueuedEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(
                jobQueuedEvent,
                jobQueuedEvent.GetType())
        };

        _logger.LogInformation("Publishing {EventType} for job {JobId} to queue {QueueName}",
            envelope.EventType,
            jobQueuedEvent.JobId,
            queueName);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(envelope));

        var properties = new BasicProperties
        {
            Persistent = true
        };

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published {EventType} for job {JobId} to queue {QueueName}",
            envelope.EventType,
            jobQueuedEvent.JobId,
            queueName);
    }

    private string Resolve(JobType jobType)
    {
        return jobType switch
        {
            JobType.Email => _options.EmailJobQueueName,
            JobType.PdfGeneration => _options.PdfGenerationJobQueueName,
            JobType.ReportGeneration => _options.ReportGenerationJobQueueName,

            _ => throw new NotSupportedException($"Queue for job type {jobType} is not configured")
        };
    }
}