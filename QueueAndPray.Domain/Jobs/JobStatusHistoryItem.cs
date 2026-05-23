namespace QueueAndPray.Domain.Jobs;

public class JobStatusHistoryItem
{
    public JobStatus Status { get; set; }
    public string? Result { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
