using System.Text;
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

        using var tx = await db.Database.BeginTransactionAsync(ct);

        var events = await db.EventOutbox
            .FromSqlRaw(@"
            SELECT *
            FROM event_outbox
            WHERE processed_at IS NULL
              AND locked_at IS NULL
            ORDER BY occurred_at
            FOR UPDATE SKIP LOCKED
            LIMIT 10
        ")
            .ToListAsync(ct);

        if (events.Count == 0)
            return;

        var now = DateTime.UtcNow;

        foreach (var evt in events)
        {
            evt.LockedAt = now;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // 🔽 publish OUTSIDE the transaction
        foreach (var evt in events)
        {
            try
            {
                await publisher.PublishAsync(
                    routingKey: "timecard.created",
                    message: evt.Payload
                );

                evt.ProcessedAt = DateTime.UtcNow;
                evt.LockedAt = null;

                await db.SaveChangesAsync(ct);
            }
            catch
            {
                // release lock for retry
                evt.LockedAt = null;
                await db.SaveChangesAsync(ct);
                throw;
            }
        }
    }
}