using Microsoft.EntityFrameworkCore;
using TimecardService.BackgroundServices;
using TimecardService.Data;
using TimecardService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TimecardDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Configuration
.AddJsonFile("appsettings.json", false)
.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
.AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddHostedService<OutboxPublisherService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();