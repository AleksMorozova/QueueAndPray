namespace QueueAndPray.Application.Jobs.Events;

public sealed class EmailJobQueuedEvent : IJobQueuedEvent
{
    public Guid JobId { get; init; }

    public string Payload { get; init; } = default!;

    public DateTime QueuedAtUtc { get; init; }
}