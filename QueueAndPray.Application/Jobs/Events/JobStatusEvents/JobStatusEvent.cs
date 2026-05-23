using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Events.JobStatusEvents;

public class JobStatusEvent
{
    public Guid JobId { get; set; }
    public DateTime ProceedsAtUtc { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; }
    public string Reason { get; set; }
}
