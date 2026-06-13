using Microsoft.EntityFrameworkCore;
using QueueAndPray.Abstractions.Jobs.Abstractions;
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
        var now = DateTime.UtcNow;
        var lockedUntilUtc = now.AddMinutes(2);

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var entities = await _dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM "OutboxMessages"
                WHERE "PublishedAtUtc" IS NULL
                  AND ("LockedUntilUtc" IS NULL OR "LockedUntilUtc" < {now})
                  AND ("NextAttemptAtUtc" IS NULL OR "NextAttemptAtUtc" <= {now})
                ORDER BY "CreatedAtUtc"
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (var entity in entities)
        {
            entity.LockedUntilUtc = lockedUntilUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return entities
            .Select(x => x.ToDomain())
            .ToList();
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
        entity.LockedUntilUtc = message.LockedUntilUtc;
        entity.NextAttemptAtUtc = message.NextAttemptAtUtc;
        entity.AttemptCount = message.AttemptCount;
        entity.Error = message.Error;

        await Task.CompletedTask;
    }
}
