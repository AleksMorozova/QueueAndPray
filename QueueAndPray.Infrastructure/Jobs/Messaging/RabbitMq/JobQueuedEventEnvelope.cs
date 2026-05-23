namespace QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;

public sealed class JobQueuedEventEnvelope
{
    public string EventType { get; set; } = default!;

    public string Payload { get; set; } = default!;
}