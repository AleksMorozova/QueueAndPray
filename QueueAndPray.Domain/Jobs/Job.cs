namespace QueueAndPray.Domain.Jobs;

public class Job
{
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
