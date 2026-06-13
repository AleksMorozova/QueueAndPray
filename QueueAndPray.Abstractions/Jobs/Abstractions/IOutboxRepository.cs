using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);

    Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken);
}
