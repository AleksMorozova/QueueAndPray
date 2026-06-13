using Microsoft.EntityFrameworkCore;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Mappers;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Repositories;

public sealed class EfJobRepository : IJobRepository
{
    private readonly QueueAndPrayDbContext _dbContext;

    public EfJobRepository(
        QueueAndPrayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(
        Job job,
        CancellationToken cancellationToken)
    {
        var entity = job.ToEntity();

        _dbContext.Jobs.Add(entity);

        return Task.CompletedTask;
    }

    public async Task<Job?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Jobs
            .AsNoTracking()
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == id,cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyCollection<Job>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Jobs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => x.ToDomain())
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(
        Job job,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Jobs
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == job.Id, cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException(
                $"Job '{job.Id}' was not found.");
        }

        entity.UpdateEntity(job);

        var existingHistoryIds = entity.StatusHistory
            .Select(x => x.Id)
            .ToHashSet();

        var newHistoryItems = job.StatusHistory
            .Where(x => !existingHistoryIds.Contains(x.Id))
            .Select(x => x.ToEntity())
            .ToList();

        if (newHistoryItems.Count > 0)
        {
            await _dbContext.JobStatusHistory.AddRangeAsync(
                newHistoryItems,
                cancellationToken);
        }

        await Task.CompletedTask;
    }
}
