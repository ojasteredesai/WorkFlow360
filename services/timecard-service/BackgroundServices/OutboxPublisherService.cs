using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TimecardService.Data;
using TimecardService.Messaging.Abstractions;

namespace TimecardService.BackgroundServices;

public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox publisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingEvents(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publishing failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // This should come from config.
        }
    }

    private async Task PublishPendingEvents(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TimecardDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var events = await db.EventOutbox
            .Where(e => e.ProcessedAt == null)
            .OrderBy(e => e.OccurredAt)
            .Take(10) // This should come from config.
            .ToListAsync(ct);

        foreach (var evt in events)
        {
            try
            {
                await publisher.PublishAsync(
                    routingKey: "timecard.created",
                    message: JsonDocument.Parse(evt.Payload).RootElement
                );

                evt.ProcessedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to publish outbox event {EventId}. Will retry.",
                    evt.Id);

                // DO NOT mark processed
                // retry will happen naturally
            }
        }
    }
}