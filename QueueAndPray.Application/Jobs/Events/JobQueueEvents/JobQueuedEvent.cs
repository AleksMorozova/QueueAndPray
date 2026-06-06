using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Events.JobQueueEvents;

public class JobQueuedEvent
{
    public Guid JobId { get; set; }

    public Guid MessageId { get; init; }

    public string Payload { get; set; } = default!;

    public JobType Type { get; set; }

    public DateTime QueuedAtUtc { get; set; }
}