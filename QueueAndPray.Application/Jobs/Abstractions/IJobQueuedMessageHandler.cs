namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobQueuedMessageHandler
{
    Task HandleAsync(string message, CancellationToken cancellationToken);
}