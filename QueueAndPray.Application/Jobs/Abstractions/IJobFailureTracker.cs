namespace QueueAndPray.Application.Jobs.Abstractions;

public interface IJobFailureTracker
{
    Task TrackRetryAttemptAsync(
        Guid jobId,
        int attempt,
        string reason,
        CancellationToken cancellationToken);

    Task TrackDeadLetterAsync(
        Guid jobId,
        string reason,
        CancellationToken cancellationToken);
}