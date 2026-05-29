using QueueAndPray.Application.Jobs.Abstractions;

namespace QueueAndPray.Application.Jobs.Tracking;

public sealed class JobFailureTracker : IJobFailureTracker
{
    private readonly IJobRepository _jobRepository;

    public JobFailureTracker(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public Task TrackRetryAttemptAsync(
        Guid jobId,
        int attempt,
        string reason,
        CancellationToken cancellationToken)
    {
        return _jobRepository.IncrementRetryCountAsync(
            jobId,
            attempt,
            cancellationToken);
    }

    public Task TrackDeadLetterAsync(
        Guid jobId,
        string reason,
        CancellationToken cancellationToken)
    {
        return _jobRepository.MarkAsDeadLetteredAsync(
            jobId,
            reason,
            cancellationToken);
    }
}