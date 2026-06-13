using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IJobStatusPublisher
{
    Task PublishAsync(
        Guid jobId,
        JobType type,
        JobStatus status,
        string? reason,
        CancellationToken cancellationToken);
}
