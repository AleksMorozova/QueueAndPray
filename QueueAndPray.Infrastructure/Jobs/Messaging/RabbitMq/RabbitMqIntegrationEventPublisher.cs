using Microsoft.Extensions.Options;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Infrastructure.Jobs.Options;
using RabbitMQ.Client;
using System.Text;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;

    public RabbitMqIntegrationEventPublisher(
        RabbitMqConnectionFactory connectionFactory,
        IOptions<RabbitMqOptions> options)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
    }

    public async Task PublishAsync(
        string routingKey,
        string payload,
        CancellationToken cancellationToken)
    {
        await using var connection =
            await _connectionFactory.CreateConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.EventsExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(payload);

        await channel.BasicPublishAsync(
            exchange: _options.EventsExchangeName,
            routingKey: routingKey,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken);
    }
}
