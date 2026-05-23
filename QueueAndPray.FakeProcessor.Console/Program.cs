using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};

await using var connection = await factory.CreateConnectionAsync();
await using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "queueandpray.job-status",
    durable: true,
    exclusive: false,
    autoDelete: false);

Console.WriteLine("QueueAndPray fake external processor started.");
Console.WriteLine("Paste JobId or type 'exit'.");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
        break;

    if (!Guid.TryParse(input, out var jobId))
    {
        Console.WriteLine("Invalid JobId.");
        continue;
    }

    var isSuccess = Random.Shared.NextDouble() >= 0.35;

    var statusEvent = new JobStatusEvent
    {
        JobId = jobId,
        Type = JobType.Email, // пока руками
        Status = isSuccess ? JobStatus.Completed : JobStatus.Failed,
        ProcessedAtUtc = DateTime.UtcNow,
        Reason = isSuccess ? null : "Fake external processor failed randomly"
    };

    var json = JsonSerializer.Serialize(statusEvent);
    var body = Encoding.UTF8.GetBytes(json);

    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: "queueandpray.job-status",
        mandatory: false,
        body: body);

    Console.WriteLine(
        $"Published {statusEvent.Status} status for job {jobId}");
}

public sealed class JobStatusEvent
{
    public Guid JobId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; }
    public string? Reason { get; set; }
}

public enum JobType
{
    Email = 1,
    PdfGeneration = 2,
    ReportGeneration = 3
}

public enum JobStatus
{
    Queued = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}