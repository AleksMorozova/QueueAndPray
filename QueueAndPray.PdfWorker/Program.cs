using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueAndPray.Abstractions.Common.Resilience;
using QueueAndPray.Abstractions.Jobs.Abstractions;
using QueueAndPray.Infrastructure.Jobs.Messaging.RabbitMq;
using QueueAndPray.Infrastructure.Jobs.Options;
using QueueAndPray.Infrastructure.Jobs.Persistence;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF;
using QueueAndPray.Infrastructure.Jobs.Persistence.EF.Repositories;
using QueueAndPray.Infrastructure.Resilience;
using QueueAndPray.Infrastructure.Jobs.Messaging.Outbox;
using QueueAndPray.PdfWorker.Processing;
using QueueAndPray.PdfWorker.Workers.Messaging;

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
builder.Services.AddSingleton<PdfJobProcessor>();
builder.Services.AddScoped<PdfJobOrchestrator>();
builder.Services.AddScoped<PdfJobQueuedMessageHandler>();

builder.Services.AddHostedService<RabbitMqPdfJobConsumerWorker>();

await builder.Build().RunAsync();
