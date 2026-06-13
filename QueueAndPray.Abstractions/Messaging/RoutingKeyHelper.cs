using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Abstractions.Messaging;

public static class RoutingKeyHelper
{
    public static string Queued(JobType jobType)
        => $"job.{GetJobType(jobType)}.queued";

    public static string Started(JobType jobType)
        => $"job.{GetJobType(jobType)}.started";

    public static string Completed(JobType jobType)
        => $"job.{GetJobType(jobType)}.completed";

    public static string Failed(JobType jobType)
        => $"job.{GetJobType(jobType)}.failed";

    private static string GetJobType(JobType jobType)
    {
        return jobType switch
        {
            JobType.Email => "email",
            JobType.PdfGeneration => "pdf",
            JobType.ReportGeneration => "report",
            _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, null)
        };
    }
}
