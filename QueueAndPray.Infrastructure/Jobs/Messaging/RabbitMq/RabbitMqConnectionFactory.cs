using Microsoft.Extensions.Options;
using QueueAndPray.Infrastructure.Jobs.Options;
using RabbitMQ.Client;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;

public sealed class RabbitMqConnectionFactory
{
    private readonly RabbitMqOptions _options;

    public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        Console.WriteLine("Creating RabbitMQ connection...");

        var connection = await factory.CreateConnectionAsync();

        Console.WriteLine($"Connection created: {connection.IsOpen}");

        return connection;
    }
}