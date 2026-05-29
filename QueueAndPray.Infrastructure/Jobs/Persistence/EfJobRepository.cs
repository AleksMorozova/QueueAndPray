using Microsoft.EntityFrameworkCore;
using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueAndPray.Infrastructure.Jobs.Persistence;

public sealed class EfJobRepository : IJobRepository
{
    private readonly QueueAndPrayDbContext _dbContext;

    public EfJobRepository(QueueAndPrayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken)
    {
        try 
        {
            var entity = new JobEntity
            {
                Id = job.Id,
                Description = job.Description,
                Payload = job.Payload,
                Type = job.Type,
                Status = job.Status,
                Result = job.Result,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Jobs.Add(entity);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new AppException("job_not_saved", $"Job '{job.Id}' was not created. The database refused to cooperate. Maybe it needs coffee. Base exception: {ex.Message}");
        }
    }

    public async Task<IReadOnlyCollection<Job>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Jobs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new Job(
                x.Id,
                x.Description,
                x.Payload,
                x.Type,
                x.Status,
                x.Result))
            .ToListAsync(cancellationToken);
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (entity is null)
        {
            return null;
        }

        return new Job(
            entity.Id,
            entity.Description,
            entity.Payload,
            entity.Type,
            entity.Status,
            entity.Result);
    }

    public async Task UpdateStatusAsync(Guid jobId, JobStatus status, string? result, CancellationToken cancellationToken)
    {
        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);

        if (job is null)
            throw new InvalidOperationException($"Job '{jobId}' was not found.");

        job.Status = status;
        job.Result = result;
        job.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task IncrementRetryCountAsync(Guid jobId, int attempt, CancellationToken cancellationToken)
    {
        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);

        if (job is null)
        {
            return;
        }

        job.RetryCount = attempt;

        job.FirstFailedAtUtc ??= DateTime.UtcNow;
        job.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsDeadLetteredAsync(
    Guid jobId,
    string reason,
    CancellationToken cancellationToken)
    {
        var job = await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);

        if (job is null)
        {
            return;
        }

        job.Status = JobStatus.Failed;
        job.Result = $"THIS MESSAGE HAS SUFFERED ENOUGH. {reason}";
        job.DeadLetteredAtUtc = DateTime.UtcNow;
        job.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}