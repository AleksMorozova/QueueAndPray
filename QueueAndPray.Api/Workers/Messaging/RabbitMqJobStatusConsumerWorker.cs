using Microsoft.Extensions.Options;
using QueueAndPray.Application.Common.Messaging;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobStatusEvents;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace QueueAndPray.Api.Workers.Messaging;

public sealed class RabbitMqJobStatusConsumerWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqJobStatusConsumerWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RabbitMqJobStatusConsumerWorker(
        RabbitMqConnectionFactory connectionFactory,
        ILogger<RabbitMqJobStatusConsumerWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var connection =
                await _connectionFactory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(
                exchange: MessagingTopology.EventsExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: MessagingTopology.JobStatusQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: MessagingTopology.JobStatusQueueName,
                exchange: MessagingTopology.EventsExchangeName,
                routingKey: JobBindingPatterns.AnyStatus,
                cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await channel.BasicGetAsync(
                    queue: MessagingTopology.JobStatusQueueName,
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

                    var envelope = JsonSerializer.Deserialize<JobStatusEventEnvelope>(message);

                    if (envelope is null)
                    {
                        throw new InvalidOperationException("RabbitMQ status envelope is empty. QueueAndPray is confused.");
                    }

                    _logger.LogInformation(
                        "Received {EventType} for job {JobId}",
                        envelope.Type,
                        envelope.JobId);

                    await using var scope = _scopeFactory.CreateAsyncScope();

                    var processor = scope.ServiceProvider
                        .GetRequiredService<IJobStatusProcessor>();

                    await processor.DispatchAsync(new JobStatusEvent
                    {
                        Type = envelope.Type,
                        JobId = envelope.JobId,
                        Status = envelope.Status,
                        Reason = envelope.Reason,
                        ProceedsAtUtc = envelope.ProceedsAtUtc,
                    }, stoppingToken);

                    await channel.BasicAckAsync(
                        deliveryTag: result.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process RabbitMQ status message");

                    await channel.BasicNackAsync(
                        deliveryTag: result.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: CancellationToken.None);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("RabbitMQ job status consumer is stopping gracefully.");
        }
    }
}