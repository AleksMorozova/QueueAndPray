namespace QueueAndPray.Abstractions.Jobs.Abstractions;

public interface IOutboxProcessor
{
    Task ProcessPendingAsync(CancellationToken cancellationToken);
}