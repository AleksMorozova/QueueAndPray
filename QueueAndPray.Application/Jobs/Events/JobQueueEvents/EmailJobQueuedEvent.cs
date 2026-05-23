using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Events.JobQueueEvents;

public sealed class EmailJobQueuedEvent : IJobQueuedEvent
{
    public string Payload { get; init; } = default!;

    public DateTime QueuedAtUtc { get; init; }
    public Guid JobId { get; set; }
    public JobType Type { get; set; }
}