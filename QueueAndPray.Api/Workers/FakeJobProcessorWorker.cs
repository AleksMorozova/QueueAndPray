using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Infrastructure.Jobs.Messaging;

namespace QueueAndPray.Api.Workers;

public class FakeJobProcessorWorker : BackgroundService
{
    private readonly InMemoryJobQueue _jobQueue;
    private readonly IJobProcessingDispatcher _dispatcher;
    private readonly ILogger<FakeJobProcessorWorker> _logger;

    private readonly SemaphoreSlim _semaphore = new(3);

    public FakeJobProcessorWorker(
        InMemoryJobQueue jobQueue,
        IJobProcessingDispatcher dispatcher,
        ILogger<FakeJobProcessorWorker> logger)
    {
        _jobQueue = jobQueue;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobQueuedEvent in _jobQueue.Reader.ReadAllAsync(stoppingToken))
        {
            await _semaphore.WaitAsync(stoppingToken);

            _ = ProcessInBackgroundAsync(jobQueuedEvent, stoppingToken);
        }
    }

    private async Task ProcessInBackgroundAsync(
        IJobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            await _dispatcher.DispatchAsync(jobQueuedEvent, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process job event {EventType} for job {JobId}",
                jobQueuedEvent.GetType().Name,
                jobQueuedEvent.JobId);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}