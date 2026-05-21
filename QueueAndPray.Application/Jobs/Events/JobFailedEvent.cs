namespace QueueAndPray.Application.Jobs.Events;

public class JobFailedEvent
{
    public Guid JobId { get; init; }
    public string Reason { get; init; } = default!;
    public DateTime FailedAtUtc { get; init; }
}
