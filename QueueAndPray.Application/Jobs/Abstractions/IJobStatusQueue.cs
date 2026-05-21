using QueueAndPray.Application.Jobs.Events;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobStatusQueue
{
    Task PublishProcessedAsync(JobProcessedEvent jobProcessedEvent, CancellationToken cancellationToken);

    Task PublishFailedAsync(JobFailedEvent jobFailedEvent, CancellationToken cancellationToken);
}
