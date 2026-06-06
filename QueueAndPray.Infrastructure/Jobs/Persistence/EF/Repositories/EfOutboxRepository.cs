using Microsoft.EntityFrameworkCore;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Domain.Jobs;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Mappers;

namespace QueueAndPray.Infrastructure.Jobs.Persistence.EF.Repositories;

public class EfOutboxRepository : IOutboxRepository
{
    private readonly QueueAndPrayDbContext _dbContext;

    public EfOutboxRepository(
        QueueAndPrayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.Add(
            message.ToEntity());

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await _dbContext.OutboxMessages
            .AsNoTracking()
            .Where(x => x.PublishedAtUtc == null)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(batchSize)
            .Select(x => x.ToDomain())
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.OutboxMessages
            .FirstOrDefaultAsync(
                x => x.Id == message.Id,
                cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException(
                $"Outbox message '{message.Id}' was not found.");
        }

        entity.PublishedAtUtc = message.PublishedAtUtc;
        entity.Error = message.Error;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
