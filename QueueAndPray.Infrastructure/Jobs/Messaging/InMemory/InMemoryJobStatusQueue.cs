using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using System.Threading.Channels;

namespace QueueAndPray.Infrastructure.Jobs.Messaging.InMemory;

public sealed class InMemoryJobStatusQueue //: IJobStatusEventPublisher
{
    //private readonly Channel<object> _channel = Channel.CreateUnbounded<object>();

    //public ChannelReader<object> Reader => _channel.Reader;

    //public async Task PublishProcessedAsync(JobCompletedEvent jobProcessedEvent, CancellationToken cancellationToken)
    //{
    //    await _channel.Writer.WriteAsync(jobProcessedEvent, cancellationToken);
    //}

    //public async Task PublishFailedAsync(JobFailedEvent jobFailedEvent, CancellationToken cancellationToken)
    //{
    //    await _channel.Writer.WriteAsync(jobFailedEvent, cancellationToken);
    //}
}