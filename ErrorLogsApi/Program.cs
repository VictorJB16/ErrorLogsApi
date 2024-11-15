using Application.Services;
using Domain.Interfaces;
using ErrorLogsApi.BackgroundServices;
using ErrorLogsApi.Jobs;
using Infrastucture.Repositories;
using Infrastucture.Settings;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AzureServiceBusSettings>(
    builder.Configuration.GetSection("AzureServiceBus"));

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
builder.Services.AddScoped<ErrorLogService>();

// Add Quartz for the RetryJob
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Register the retry job
    var jobKey = new JobKey("RetryJob");

    q.AddJob<RetryJob>(opts => opts.WithIdentity(jobKey));

    // Cambiar la expresión cron para ejecutarse cada 10 segundos
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("RetryJob-trigger")
        .WithCronSchedule("*/10 * * * * ?")); // Cada 10 segundos
});

// Ensure Quartz hosted service waits for jobs to complete
builder.Services.AddQuartzHostedService(
    q => q.WaitForJobsToComplete = true);

// Add HTTP client factory (needed for HTTP calls in RetryJob)
builder.Services.AddHttpClient();

// Add controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Frontend port
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
