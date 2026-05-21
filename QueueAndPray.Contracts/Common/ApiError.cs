namespace QueueAndPray.Contracts.Common;

public class ApiError
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public Dictionary<string, string[]>? Details { get; init; }
}
