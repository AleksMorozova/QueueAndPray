using Microsoft.Extensions.Options;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using QueueAndPray.Infrastructure.Jobs.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;

public sealed class RabbitMqJobQueue : IJobQueue
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;

    public RabbitMqJobQueue(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
    }

    public async Task PublishAsync(
        IJobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var envelope = new JobQueuedEventEnvelope
        {
            EventType = jobQueuedEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(
                jobQueuedEvent,
                jobQueuedEvent.GetType())
        };

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(envelope));

        var properties = new BasicProperties
        {
            Persistent = true
        };

        Console.WriteLine($"Declaring queue: '{_options.JobQueueName}'");

        await channel.QueueDeclareAsync(
            queue: _options.JobQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        Console.WriteLine($"Queue declared: '{_options.JobQueueName}'");

        Console.WriteLine("Publishing message...");

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _options.JobQueueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        Console.WriteLine("Message published.");
    }
}