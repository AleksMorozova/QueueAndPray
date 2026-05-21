using QueueAndPray.Api.Workers;
using QueueAndPray.Application.Jobs.Abstractions;
using QueueAndPray.Application.Jobs.Services;
using QueueAndPray.Infrastructure.Jobs.Messaging;
using QueueAndPray.Infrastructure.Jobs.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IJobService, JobService>();
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();

builder.Services.AddSingleton<InMemoryJobQueue>();
builder.Services.AddSingleton<IJobQueue>(sp => sp.GetRequiredService<InMemoryJobQueue>());

builder.Services.AddSingleton<InMemoryJobStatusQueue>();
builder.Services.AddSingleton<IJobStatusQueue>(sp => sp.GetRequiredService<InMemoryJobStatusQueue>());

builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();

builder.Services.AddHostedService<FakeJobProcessorWorker>();
builder.Services.AddHostedService<JobProcessingEventConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
