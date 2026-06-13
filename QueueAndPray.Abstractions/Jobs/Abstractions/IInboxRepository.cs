using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(
        Guid messageId,
        CancellationToken cancellationToken);

    Task AddAsync(
        InboxMessage message,
        CancellationToken cancellationToken);
}