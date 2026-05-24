using QueueAndPray.Application.Jobs.Events.JobStatusEvents;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobStatusProcessor
{
    Task DispatchAsync(
        JobStatusEvent jobStatusEvent,
        CancellationToken cancellationToken);
}