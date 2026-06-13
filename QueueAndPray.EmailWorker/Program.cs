using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueAndPray.Abstractions.Common.Resilience;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Infrastructure.Jobs.Messaging.Outbox;
using QueueAndPray.EmailWorker.Processing;
using QueueAndPray.EmailWorker.Workers.Messaging;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using QueueAndPray.Infrastructure.Jobs.Persistence;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Repositories;
using QueueAndPray.Infrastructure.Resilience;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<QueueAndPrayDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("QueueAndPrayDb"));
});

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.Configure<RabbitMqRetryOptions>(
    builder.Configuration.GetSection("RabbitMqRetry"));

builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddSingleton<RabbitMqJobQueueConsumer>();

builder.Services.AddScoped<IOutboxRepository, EfOutboxRepository>();
builder.Services.AddScoped<IInboxRepository, EfInboxRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddScoped<IJobStatusPublisher, OutboxJobStatusPublisher>();
builder.Services.AddSingleton<IRetryPolicyExecutor, RetryPolicyExecutor>();
builder.Services.AddSingleton<IEmailJobProcessor, FakeEmailJobProcessor>();
builder.Services.AddScoped<IEmailJobOrchestrator, EmailJobOrchestrator>();
builder.Services.AddScoped<IEmailJobQueuedMessageHandler, EmailJobQueuedMessageHandler>();

builder.Services.AddHostedService<RabbitMqEmailJobConsumerWorker>();

await builder.Build().RunAsync();
