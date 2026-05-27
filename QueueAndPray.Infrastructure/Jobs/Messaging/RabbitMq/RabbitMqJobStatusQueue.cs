using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobStatusEvents;
using QueueAndPray.Infrastructure.Jobs.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;

public sealed class RabbitMqJobStatusQueue : IJobStatusQueue
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqJobStatusQueue> _logger;

    public RabbitMqJobStatusQueue(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqJobStatusQueue> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(
        JobStatusEvent jobStatusEvent,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.JobStatusQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(jobStatusEvent));

        var properties = new BasicProperties
        {
            Persistent = true
        };

        _logger.LogInformation(
            "Publishing status {Status} for job {JobId} to queue {QueueName}",
            jobStatusEvent.Status,
            jobStatusEvent.JobId,
            _options.JobStatusQueueName);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.JobStatusQueueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}