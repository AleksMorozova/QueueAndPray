using QueueAndPray.Api.Middleware;
using QueueAndPray.Api.Workers;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Dispatchers;
using QueueAndPray.Application.Jobs.Events;
using QueueAndPray.Application.Jobs.Processing;
using QueueAndPray.Application.Jobs.Services;
using QueueAndPray.Infrastructure.Jobs.Messaging;
using QueueAndPray.Infrastructure.Jobs.Persistence;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application services
builder.Services.AddSingleton<IJobService, JobService>();
builder.Services.AddSingleton<IJobDispatcher, JobDispatcher>();

// In-memory persistence
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();

// In-memory queues
builder.Services.AddSingleton<InMemoryJobQueue>();
builder.Services.AddSingleton<IJobQueue>(sp =>
    sp.GetRequiredService<InMemoryJobQueue>());

builder.Services.AddSingleton<InMemoryJobStatusQueue>();
builder.Services.AddSingleton<IJobStatusQueue>(sp =>
    sp.GetRequiredService<InMemoryJobStatusQueue>());

// Job processing
builder.Services.AddSingleton<IJobProcessingDispatcher, JobProcessingDispatcher>();

builder.Services.AddSingleton<
    IJobProcessor<EmailJobQueuedEvent>,
    EmailJobProcessor>();

builder.Services.AddSingleton<
    IJobProcessor<PdfGenerationJobQueuedEvent>,
    PdfGenerationJobProcessor>();

builder.Services.AddSingleton<
    IJobProcessor<ReportGenerationJobQueuedEvent>,
    ReportGenerationJobProcessor>();

// Background workers
builder.Services.AddHostedService<FakeJobProcessorWorker>();
builder.Services.AddHostedService<JobProcessingEventConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();