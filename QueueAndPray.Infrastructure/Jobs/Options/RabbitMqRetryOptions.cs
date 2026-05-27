namespace QueueAndPray.Infrastructure.Jobs.Options;

public class RabbitMqRetryOptions
{
    public int MaxRetryAttempts { get; set; } = 3;

    public int DelayMilliseconds { get; set; } = 1000;
}