using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Abstractions.Jobs.Events.JobStatusEvents;

public class JobStatusEventEnvelope
{
    public Guid MessageId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid? CausationId { get; set; }
    public Guid JobId { get; set; }
    public DateTime ProceedsAtUtc { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; }
    public string? Reason { get; set; }
}
