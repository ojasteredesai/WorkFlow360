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

    // keep polling interval explicit (later move to config)
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RetryDelay   = TimeSpan.FromSeconds(5);

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
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
                break;
            }
            catch (Exception ex)
            {
                // 🔑 KEY CHANGE:
                // Do NOT crash, do NOT stop the service.
                // Treat this as infra unavailability (RabbitMQ, network, etc.)
                _logger.LogWarning(
                    ex,
                    "Outbox publishing failed (likely infra not ready). Will retry.");

                await Task.Delay(RetryDelay, stoppingToken);
            }
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
            catch (Exception ex)
            {
                // 🔑 KEY CHANGE:
                // Release lock and let retry loop handle recovery.
                _logger.LogWarning(
                    ex,
                    "Failed to publish outbox event {EventId}. Releasing lock.",
                    evt.Id);

                evt.LockedAt = null;
                await db.SaveChangesAsync(ct);

                // bubble up to outer loop → retry with delay
                throw;
            }
        }
    }
}