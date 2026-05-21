namespace QueueAndPray.Domain.Jobs;

public class Job
{
    public Guid Id { get; set; }

    public string Description { get; set; } = default!;

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public string Reason { get; set; } = default!;

    public string Result { get; set; } = default!;

    public JobStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
