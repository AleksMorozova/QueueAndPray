using QueueAndPray.Domain.Jobs;

namespace QueueAndPray.Application.Common.Messaging;
public static class MessagingTopology
{
    public const string EventsExchangeName = "queueandpray.events";

    public const string JobStatusQueueName = "queueandpray.job-status";

    public static string JobQueue(JobType jobType)
    {
        return jobType switch
        {
            JobType.Email => "queueandpray.email.jobs",
            JobType.PdfGeneration => "queueandpray.pdf.jobs",
            JobType.ReportGeneration => "queueandpray.report.jobs",
            _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, null)
        };
    }
}