using Microsoft.Extensions.Options;
using QueueAndPray.Abstractions.Messaging;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using System.Text;
using System.Text.Json;

namespace QueueAndPray.Api.Workers.Messaging;

public sealed class RabbitMqDeadLetterEmailWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqDeadLetterEmailWorker> _logger;

    public RabbitMqDeadLetterEmailWorker(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqDeadLetterEmailWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var deadLetterQueue = $"{MessagingTopology.JobQueue(JobType.Email)}.dead-letter";

        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: deadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Dead-letter worker started for queue {QueueName}",
            deadLetterQueue);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await channel.BasicGetAsync(
                queue: deadLetterQueue,
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

                var envelope = JsonSerializer.Deserialize<JobQueuedEventEnvelope>(
                    message);

                if (envelope is null)
                {
                    throw new InvalidOperationException("Dead-letter envelope is empty. Even the failures gave up.");
                }

                var payload = JsonSerializer.Deserialize<JobQueuedEvent>(
                    envelope.Payload);

                if (payload is null)
                {
                    throw new InvalidOperationException("Dead-letter payload is empty. QueueAndPray lost the corpse.");
                }

                _logger.LogWarning("Dead-letter email message received for job {JobId}. The message has suffered enough.", payload.JobId);

                await using var scope = _scopeFactory.CreateAsyncScope();

                var jobStatusPublisher = scope.ServiceProvider
                    .GetRequiredService<IJobStatusPublisher>();
                var unitOfWork = scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();

                await jobStatusPublisher.PublishAsync(
                    payload.JobId,
                    payload.Type,
                    Domain.Jobs.JobStatus.DeadLettered,
                    "Email job message reached dead-letter queue. It has suffered enough.",
                    stoppingToken);

                await unitOfWork.SaveChangesAsync(stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: result.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Dead-letter queue processing failed. Even suffering failed.");

                await channel.BasicNackAsync(
                    deliveryTag: result.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        }
    }
}
