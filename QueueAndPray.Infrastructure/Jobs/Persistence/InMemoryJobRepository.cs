using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Domain.Jobs;
using System.Collections.Concurrent;

namespace QueueAndPray.Infrastructure.Jobs.Persistence;

public class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, Job> _jobs = new();

    public Task AddAsync(Job job, CancellationToken cancellationToken)
    {
        if (job.Description.Contains(
           "error",
           StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("job_validation_error", "Job description contains forbidden word 'error'.");
        }

        _jobs[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task<IReadOnlyCollection<Job>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Job> jobs = _jobs.Values.ToList();

        return Task.FromResult(jobs);
    }

    public Task UpdateStatusAsync(Guid id, JobStatus status, string result, CancellationToken cancellationToken)
    {
        if (_jobs.TryGetValue(id, out var job))
        {
            job.Status = status;
            job.Result = result;

            job.StatusHistory.Add(new JobStatusHistoryItem
            {
                Status = status,
                Result = result,
                ChangedAtUtc = DateTime.UtcNow
            });

            if (status is JobStatus.Completed or JobStatus.Failed)
            {
                job.CompletedAtUtc = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }
}