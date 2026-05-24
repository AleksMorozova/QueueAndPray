namespace QueueAndPray.Domain.Jobs;

public class Job
{
    private JobType type;
    private JobStatus status;

    public Job(Guid id, string description, string payload, JobType type, JobStatus status, string? result)
    {
        Id = id;
        Description = description;
        Payload = payload;
        this.type = type;
        this.status = status;
        Result = result;
    }

    public Guid Id { get; set; }

    public string Description { get; set; } = default!;

    public JobType Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public string Result { get; set; } = default!;

    public JobStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime CompletedAtUtc { get; set; }
    public List<JobStatusHistoryItem> StatusHistory { get; set; } = [];
}
