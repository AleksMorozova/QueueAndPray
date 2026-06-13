namespace QueueAndPray.Domain.Jobs;

public class JobStatusHistoryItem
{
    private JobStatusHistoryItem()
    {
    }

    public JobStatusHistoryItem(
        Guid id,
        Guid jobId,
        JobStatus status,
        string? result,
        DateTime changedAtUtc)
    {
        Id = id;
        JobId = jobId;
        Status = status;
        Result = result;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid JobId { get; private set; }

    public JobStatus Status { get; private set; }

    public string? Result { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    public static JobStatusHistoryItem Create(
        Guid jobId,
        JobStatus status,
        string? result)
    {
        return new JobStatusHistoryItem(
            Guid.NewGuid(),
            jobId,
            status,
            result,
            DateTime.UtcNow);
    }
}
