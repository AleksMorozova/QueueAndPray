using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobRepository
{
    Task AddAsync(Job job, CancellationToken cancellationToken);
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Job>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateStatusAsync(Guid id, JobStatus status, string? result, CancellationToken cancellationToken);
    Task IncrementRetryCountAsync(Guid jobId, int attempt, CancellationToken cancellationToken);
    Task MarkAsDeadLetteredAsync(Guid jobId, string reason, CancellationToken cancellationToken);
}