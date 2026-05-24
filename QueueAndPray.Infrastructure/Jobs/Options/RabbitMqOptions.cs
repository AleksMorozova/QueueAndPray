namespace QueueAndPray.Infrastructure.Jobs.Options;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    public string JobStatusQueueName { get; set; } = "queueandpray.job-status";
    public string EmailJobQueueName { get; set; } = "queueandpray.email.jobs";
    public string PdfGenerationJobQueueName { get; set; } = "queueandpray.pdf.jobs";
    public string ReportGenerationJobQueueName { get; set; } = "queueandpray.report.jobs";
}