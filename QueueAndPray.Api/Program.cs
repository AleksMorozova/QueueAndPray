using Microsoft.EntityFrameworkCore;
using QueueAndPray.Api.Middleware;
using QueueAndPray.Api.Workers.Messaging;
using QueueAndPray.Abstractions.Common.Resilience;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Processing;
using QueueAndPray.Application.Jobs.Services;
using QueueAndPray.Infrastructure.Jobs.Messaging.Outbox;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using QueueAndPray.Infrastructure.Jobs.Persistence;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Repositories;
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

// DB persistence
builder.Services.AddDbContext<QueueAndPrayDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("QueueAndPrayDb"));
});

builder.Services.AddScoped<IJobRepository, EfJobRepository>();
builder.Services.AddScoped<IJobRepository, EfJobRepository>();
builder.Services.AddScoped<IOutboxRepository, EfOutboxRepository>();
builder.Services.AddScoped<IInboxRepository, EfInboxRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// messaging
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.Configure<RabbitMqRetryOptions>(
    builder.Configuration.GetSection("RabbitMqRetry"));

builder.Services.AddSingleton<RabbitMqConnectionFactory>();

builder.Services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
builder.Services.AddScoped<IJobStatusPublisher, OutboxJobStatusPublisher>();

// Job processing
builder.Services.AddScoped<IJobStatusProcessor, JobStatusProcessor>();
builder.Services.AddSingleton<IRetryPolicyExecutor, RetryPolicyExecutor>();

// Outbox
builder.Services.AddScoped<IOutboxProcessor, OutboxProcessor>();

// Background workers
builder.Services.AddHostedService<RabbitMqJobStatusConsumerWorker>();
// builder.Services.AddHostedService<RabbitMqDeadLetterEmailWorker>();
builder.Services.AddHostedService<OutboxPublisherWorker>();

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
