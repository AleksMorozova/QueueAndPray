using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Events;

namespace QueueAndPray.Application.Jobs.Dispatchers;

public sealed class JobProcessingDispatcher : IJobProcessingDispatcher
{
    private readonly IJobProcessor<EmailJobQueuedEvent> _emailProcessor;
    private readonly IJobProcessor<PdfGenerationJobQueuedEvent> _pdfProcessor;
    private readonly IJobProcessor<ReportGenerationJobQueuedEvent> _reportProcessor;

    public JobProcessingDispatcher(
        IJobProcessor<EmailJobQueuedEvent> emailProcessor,
        IJobProcessor<PdfGenerationJobQueuedEvent> pdfProcessor,
        IJobProcessor<ReportGenerationJobQueuedEvent> reportProcessor)
    {
        _emailProcessor = emailProcessor;
        _pdfProcessor = pdfProcessor;
        _reportProcessor = reportProcessor;
    }

    public Task DispatchAsync(
        IJobQueuedEvent jobQueuedEvent,
        CancellationToken cancellationToken)
    {
        return jobQueuedEvent switch
        {
            EmailJobQueuedEvent email =>
                _emailProcessor.ProcessAsync(email, cancellationToken),

            PdfGenerationJobQueuedEvent pdf =>
                _pdfProcessor.ProcessAsync(pdf, cancellationToken),

            ReportGenerationJobQueuedEvent report =>
                _reportProcessor.ProcessAsync(report, cancellationToken),

            _ => throw new NotSupportedException(
                $"Job event type {jobQueuedEvent.GetType().Name} is not supported")
        };
    }
}