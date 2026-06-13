namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken);
}