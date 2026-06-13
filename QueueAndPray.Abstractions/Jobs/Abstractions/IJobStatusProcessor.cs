using QueueAndPray.Abstractions.Jobs.Events.JobStatusEvents;

namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IJobStatusProcessor
{
    Task DispatchAsync(
        JobStatusEvent jobStatusEvent,
        CancellationToken cancellationToken);
}