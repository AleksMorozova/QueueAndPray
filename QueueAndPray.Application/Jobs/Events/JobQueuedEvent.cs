namespace QueueAndPray.Application.Jobs.Events;

public class JobQueuedEvent
{
    public Guid JobId { get; init; }
    public string JobType { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DateTime QueuedAtUtc { get; init; }
}
