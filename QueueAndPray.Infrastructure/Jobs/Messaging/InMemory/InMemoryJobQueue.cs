using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using System.Threading.Channels;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.InMemory;

public sealed class InMemoryJobQueue : IJobQueue
{
    private readonly Channel<JobQueuedEvent> _channel = Channel.CreateUnbounded<JobQueuedEvent>();

    public ChannelReader<JobQueuedEvent> Reader => _channel.Reader;

    public async Task PublishAsync(
        JobQueuedEvent jobCreatedEvent,
        CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(jobCreatedEvent, cancellationToken);
    }
}