using QueueAndPray.Api.Middleware;
using QueueAndPray.Api.Workers;
using QueueAndPray.Api.Workers.Messaging;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Dispatchers;
using QueueAndPray.Application.Jobs.Processors;
using QueueAndPray.Application.Jobs.Services;
using QueueAndPray.Infrastructure.Jobs.Messaging.InMemory;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
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


builder.Services.AddSingleton<IEmailJobProcessor, FakeEmailJobProcessor>();
//IEmailJobProcessor
// In-memory persistence
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();

// messaging
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddSingleton<RabbitMqConnectionFactory>();

builder.Services.AddSingleton<IJobQueue, RabbitMqJobQueue>();

// Background workers
builder.Services.AddHostedService<RabbitMqJobStatusConsumerWorker>();
builder.Services.AddHostedService<RabbitMqEmailJobConsumerWorker>();

// Job processing
builder.Services.AddSingleton<IJobStatusProcessor, JobStatusProcessor>();

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