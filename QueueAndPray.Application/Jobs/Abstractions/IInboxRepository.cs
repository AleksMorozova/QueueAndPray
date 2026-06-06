using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IInboxRepository
{
    Task<bool> ExistsAsync(
        Guid messageId,
        CancellationToken cancellationToken);

    Task AddAsync(
        InboxMessage message,
        CancellationToken cancellationToken);
}