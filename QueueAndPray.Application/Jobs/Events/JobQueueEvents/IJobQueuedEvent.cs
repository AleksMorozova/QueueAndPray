using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Events.JobQueueEvents;

public interface IJobQueuedEvent
{
    public Guid JobId { get; set; }

    public JobType Type { get; set; }
}