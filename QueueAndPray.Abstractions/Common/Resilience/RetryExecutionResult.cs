namespace QueueAndPray.Abstractions.Common.Resilience;

public sealed record RetryExecutionResult(bool IsSuccess, string? ErrorMessage = null, Exception? FinalException = null);