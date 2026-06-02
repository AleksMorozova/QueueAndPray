namespace QueueAndPray.Application.Common.Resilience;

public sealed record RetryExecutionResult(bool IsSuccess, string? ErrorMessage = null, Exception? FinalException = null);