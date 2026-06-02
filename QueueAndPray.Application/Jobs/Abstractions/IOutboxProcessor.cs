namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IOutboxProcessor
{
    Task ProcessPendingAsync(CancellationToken cancellationToken);
}