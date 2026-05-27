using QueueAndPray.Application.Jobs.Events.JobStatusEvents;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobStatusQueue
{
    Task PublishAsync(JobStatusEvent jobStatusEvent, CancellationToken cancellationToken);
}
