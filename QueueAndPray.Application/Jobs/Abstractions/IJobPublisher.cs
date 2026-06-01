using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobPublisher
{
    Task DispatchAsync(Job job, CancellationToken cancellationToken);
}
