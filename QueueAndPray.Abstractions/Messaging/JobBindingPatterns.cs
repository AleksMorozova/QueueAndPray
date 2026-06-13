namespace QueueAndPray.Abstractions.Messaging;

public static class JobBindingPatterns
{
    public const string AnyStatus = "job.status.*";

    public const string AnyJob = "job.#";

    public const string AnyFailed = "job.*.failed";
}