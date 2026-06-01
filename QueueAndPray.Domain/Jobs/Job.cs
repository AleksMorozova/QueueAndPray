namespace QueueAndPray.Domain.Jobs;

public class Job
{
    private Job()
    {
    }

    public Job(
        string description,
        string payload,
        JobType type)
    {
        Id = Guid.NewGuid();
        Description = description;
        Payload = payload;
        Type = type;
        Status = JobStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;

        AddHistory(Status, "Job created");
    }

    public Guid Id { get; private set; }

    public string Description { get; private set; } = default!;

    public JobType Type { get; private set; }

    public string Payload { get; private set; } = default!;

    public string? Result { get; private set; }

    public JobStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public DateTime? FirstFailedAtUtc { get; private set; }

    public DateTime? DeadLetteredAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    private readonly List<JobStatusHistoryItem> _statusHistory = [];

    public IReadOnlyCollection<JobStatusHistoryItem> StatusHistory => _statusHistory;
    public void StartProcessing()
    {
        Status = JobStatus.Processing;
        UpdatedAtUtc = DateTime.UtcNow;

        AddHistory(Status, "Job is processing");
    }

    public void TrackRetryAttempt(
        int attempt,
        string reason)
    {
        RetryCount = attempt;
        Result = reason;
        FirstFailedAtUtc ??= DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        AddHistory(JobStatus.Processing, $"Retry attempt {attempt}: {reason}");
    }

    public void Complete(string result)
    {
        Status = JobStatus.Completed;
        Result = result;
        CompletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        AddHistory(Status, result);
    }

    public void DeadLetter(string reason)
    {
        Status = JobStatus.DeadLettered;
        Result = $"THIS MESSAGE HAS SUFFERED ENOUGH. {reason}";
        DeadLetteredAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        AddHistory(Status, Result);
    }

    public void Fail(string reason)
    {
        Status = JobStatus.Failed;
        Result = reason;
        UpdatedAtUtc = DateTime.UtcNow;

        AddHistory(Status, reason);
    }

    public void ApplyExternalStatus(JobStatus status, string? reason)
    {
        switch (status)
        {
            case JobStatus.Processing:
                StartProcessing();
                break;

            case JobStatus.Completed:
                Complete(reason ?? "Job completed by external status event");
                break;

            case JobStatus.Failed:
                Fail(reason ?? "Job failed by external status event");
                break;

            case JobStatus.DeadLettered:
                DeadLetter(reason ?? "Job dead-lettered by external status event");
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported external job status '{status}'.");
        }
    }

    public static Job Restore(
        Guid id,
        string description,
        string payload,
        JobType type,
        JobStatus status,
        string? result,
        int retryCount,
        DateTime createdAtUtc,
        DateTime? updatedAtUtc,
        DateTime? firstFailedAtUtc,
        DateTime? deadLetteredAtUtc,
        IEnumerable<JobStatusHistoryItem>? statusHistory = null)
    {
        var job = new Job
        {
            Id = id,
            Description = description,
            Payload = payload,
            Type = type,
            Status = status,
            Result = result,
            RetryCount = retryCount,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc,
            FirstFailedAtUtc = firstFailedAtUtc,
            DeadLetteredAtUtc = deadLetteredAtUtc
        };

        if (statusHistory is not null)
        {
            job._statusHistory.AddRange(statusHistory);
        }

        return job;
    }

    private void AddHistory(JobStatus status, string? result)
    {
        _statusHistory.Add(
            JobStatusHistoryItem.Create(
                Id,
                status,
                result));
    }
}