using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Events.JobQueueEvents;

public sealed class PdfGenerationJobQueuedEvent : IJobQueuedEvent
{
    public Guid JobId { get; set; }
    public JobType Type { get; set; }
    public string Payload { get; init; } = default!;
    public DateTime QueuedAtUtc { get; init; }
}