using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Events;

public class JobProcessedEvent
{
    public Guid JobId { get; init; }
    public DateTime ProcessedAtUtc { get; init; }
    public JobType Type { get; set; }
}
