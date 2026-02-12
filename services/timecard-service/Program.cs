using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TimecardService.BackgroundServices;
using TimecardService.Data;
using TimecardService.Messaging;
using TimecardService.Messaging.RabbitMQ;

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

// --------------------
// Build & Run
// --------------------
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
