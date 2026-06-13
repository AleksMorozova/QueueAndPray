using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QueueAndPray.Abstractions.Messaging;
using QueueAndPray.Domain.Jobs;
using RabbitMQ.Client;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;

public sealed class RabbitMqJobQueueConsumer
{
    private readonly RabbitMqConnectionFactory _connectionFactory;

    public RabbitMqJobQueueConsumer(RabbitMqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task RunAsync(
        JobType jobType,
        string workerName,
        IServiceScopeFactory scopeFactory,
        Func<IServiceProvider, string, CancellationToken, Task> handleAsync,
        ILogger logger,
        CancellationToken stoppingToken)
    {
        var queueName = MessagingTopology.JobQueue(jobType);
        var deadLetterQueue = $"{queueName}.dead-letter";

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = deadLetterQueue
        };

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
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: queueName,
            exchange: MessagingTopology.EventsExchangeName,
            routingKey: RoutingKeyHelper.Queued(jobType),
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "RabbitMQ {WorkerName} consumer started for queue {QueueName}",
            workerName,
            queueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await channel.BasicGetAsync(
                queue: queueName,
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

                logger.LogInformation(
                    "RabbitMQ {WorkerName} job message received: {Message}",
                    workerName,
                    message);

                await using var scope = scopeFactory.CreateAsyncScope();

                await handleAsync(
                    scope.ServiceProvider,
                    message,
                    stoppingToken);

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
                logger.LogError(
                    ex,
                    "Failed to process RabbitMQ {WorkerName} job message",
                    workerName);

                await channel.BasicNackAsync(
                    deliveryTag: result.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: CancellationToken.None);
            }
        }
    }
}
