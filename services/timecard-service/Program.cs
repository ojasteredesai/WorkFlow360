using Microsoft.EntityFrameworkCore;
using TimecardService.Data;

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();