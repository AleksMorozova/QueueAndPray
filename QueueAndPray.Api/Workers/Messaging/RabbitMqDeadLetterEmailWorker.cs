using Microsoft.Extensions.Options;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using System.Text;

namespace QueueAndPray.Api.Workers.Messaging;

public sealed class RabbitMqDeadLetterEmailWorker : BackgroundService
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqDeadLetterEmailWorker> _logger;

    public RabbitMqDeadLetterEmailWorker(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqDeadLetterEmailWorker> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var deadLetterQueue =
            $"{_options.EmailJobQueueName}.dead-letter";

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
                var message =
                    Encoding.UTF8.GetString(result.Body.ToArray());

                _logger.LogWarning(
                    "Dead-letter message received: {Message}",
                    message);

                // TODO:
                // save to db
                // notify
                // requeue
                // inspect
                // sacrifice another microservice

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