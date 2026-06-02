namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken);
}