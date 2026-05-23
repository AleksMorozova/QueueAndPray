using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events.JobQueueEvents;
using System.Threading.Channels;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.InMemory;

public sealed class InMemoryJobQueue : IJobQueue
{
    private readonly Channel<IJobQueuedEvent> _channel = Channel.CreateUnbounded<IJobQueuedEvent>();

    public ChannelReader<IJobQueuedEvent> Reader => _channel.Reader;

    public async Task PublishAsync(
        IJobQueuedEvent jobCreatedEvent,
        CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(jobCreatedEvent, cancellationToken);
    }
}