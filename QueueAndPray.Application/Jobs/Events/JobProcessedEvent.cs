namespace QueueAndPray.Application.Jobs.Events;

public class JobProcessedEvent
{
    public Guid JobId { get; init; }
    public DateTime ProcessedAtUtc { get; init; }
}
