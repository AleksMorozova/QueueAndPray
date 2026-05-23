using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobDispatcher
{
    Task DispatchAsync(Job job, CancellationToken cancellationToken);
}
