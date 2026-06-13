namespace QueueAndPray.EmailWorker.Processing;

public interface IEmailJobQueuedMessageHandler
{
    Task HandleAsync(string message, CancellationToken cancellationToken);
}
