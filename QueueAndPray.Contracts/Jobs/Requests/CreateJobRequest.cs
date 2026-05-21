namespace QueueAndPray.Contracts.Jobs.Requests;

public class CreateJobRequest
{
    public string Description { get; init; } = default!;

    public string Type { get; init; } = default!;

    public string Payload { get; init; } = default!;
}
