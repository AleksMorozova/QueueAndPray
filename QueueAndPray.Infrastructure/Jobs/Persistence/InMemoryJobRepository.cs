using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Domain.Jobs;
using System.Collections.Concurrent;

namespace QueueAndPray.Infrastructure.Jobs.Persistence;

public class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, Job> _jobs = new();

    public Task AddAsync(Job job, CancellationToken cancellationToken)
    {
        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task UpdateStatusAsync(Guid id, JobStatus status, string reason, CancellationToken cancellationToken)
    {
        if (_jobs.TryGetValue(id, out var job))
        {
            job.Status = status;
            job.Reason = reason;
        }

        return Task.CompletedTask;
    }
}