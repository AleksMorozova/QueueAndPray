namespace QueueAndPray.Abstractions.Jobs.Events.JobQueueEvents;

public sealed class JobQueuedEventEnvelope
{
    public string EventType { get; set; } = default!;

    public string Payload { get; set; } = default!;
}