using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TimecardService.BackgroundServices;
using TimecardService.Data;
using TimecardService.Messaging;
using TimecardService.Messaging.RabbitMQ;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Configuration
// --------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// --------------------
// Database
// --------------------
builder.Services.AddDbContext<TimecardDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------
// MVC / Swagger
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --------------------
// Messaging (existing abstraction)
// --------------------
builder.Services.AddMessaging(builder.Configuration);

// Bind RabbitMQ options so DI can resolve RabbitMqOptions
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));

// Expose RabbitMqOptions as a concrete singleton
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);

// NEW: register factory used by RabbitMqConnection
builder.Services.AddSingleton<RabbitMqConnectionFactory>();

// RabbitMQ connection (now depends on the factory)
builder.Services.AddSingleton<RabbitMqConnection>();

// --------------------
// Background services
// --------------------
builder.Services.AddHostedService<OutboxPublisherService>();
// Add HttpClient with Polly policies
builder.Services.AddHttpClient("FraudService", client =>
{
    client.BaseAddress = new Uri("https://fraud-api.example.com/");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy())
.AddPolicyHandler(GetTimeoutPolicy());
// --------------------
// Build & Run
// --------------------
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// ---------------- POLICIES ---------------- //

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (result, timeSpan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TimeoutRejectedException>()
        .CircuitBreakerAsync(
            5, // break after 5 consecutive failures
            TimeSpan.FromSeconds(30),
            onBreak: (result, breakDelay) =>
            {
                Console.WriteLine("Circuit opened!");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit closed!");
            });
}

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(5); // 5 second timeout
}
