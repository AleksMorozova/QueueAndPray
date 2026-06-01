using Microsoft.EntityFrameworkCore;
using QueueAndPray.Api.Middleware;
using QueueAndPray.Api.Workers;
using QueueAndPray.Api.Workers.Messaging;
using QueueAndPray.Application.Common.Resilience;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Orchestration;
using QueueAndPray.Application.Jobs.Processors;
using QueueAndPray.Application.Jobs.Publishers;
using QueueAndPray.Application.Jobs.Services;
using QueueAndPray.Infrastructure.Jobs.Messaging.InMemory;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using QueueAndPray.Infrastructure.Jobs.Persistence;
using QueueAndPray.Infrastructure.Resilience;
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
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobPublisher, JobPublisher>();

builder.Services.AddSingleton<IJobStatusPublishers, JobStatusPublisher>();
builder.Services.AddSingleton<IEmailJobProcessor, FakeEmailJobProcessor>();

// In-memory persistence
//builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddDbContext<QueueAndPrayDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("QueueAndPrayDb"));
});

builder.Services.AddScoped<IJobRepository, EfJobRepository>();
// messaging
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.Configure<RabbitMqRetryOptions>(
    builder.Configuration.GetSection("RabbitMqRetry"));

builder.Services.AddSingleton<RabbitMqConnectionFactory>();

builder.Services.AddSingleton<IJobQueue, RabbitMqJobQueue>();
builder.Services.AddSingleton<IJobStatusQueue, RabbitMqJobStatusQueue>();

// Background workers
builder.Services.AddHostedService<RabbitMqJobStatusConsumerWorker>();
builder.Services.AddHostedService<RabbitMqEmailJobConsumerWorker>();
builder.Services.AddHostedService<RabbitMqDeadLetterEmailWorker>();

// Job processing
builder.Services.AddScoped<IJobStatusProcessor, JobStatusProcessor>();

builder.Services.AddSingleton<IRetryPolicyExecutor, RetryPolicyExecutor>();

builder.Services.AddScoped<IEmailJobOrchestrator, EmailJobOrchestrator>();

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