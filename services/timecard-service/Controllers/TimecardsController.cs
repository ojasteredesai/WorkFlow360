using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimecardService.Data;
using TimecardService.DTOs;
using TimecardService.Events.Contracts;
using TimecardService.Messaging.Abstractions;
using TimecardService.Models;

namespace TimecardService.Controllers;

[ApiController]
[Route("api/timecards")]
public class TimecardsController : ControllerBase
{
    private readonly TimecardDbContext _db;
    private readonly IEventPublisher _eventPublisher;

    public TimecardsController(TimecardDbContext db, IEventPublisher eventPublisher)
    {
        _db = db;
        _eventPublisher = eventPublisher;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Timecards.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var tc = await _db.Timecards.FindAsync(id);
        return tc == null ? NotFound() : Ok(tc);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTimecardDto dto)
    {
        if (dto.TotalHours < 0)
            return BadRequest("Total hours cannot be negative");

        // 1️⃣ Build Timecard
        var timecard = new Timecard
        {
            Id = Guid.NewGuid(),
            WorkerId = dto.WorkerId,
            ProjectId = dto.ProjectId,
            WeekStart = dto.WeekStart,
            TotalHours = dto.TotalHours,
            Status = "SUBMITTED",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 2️⃣ Build Event (domain fact)
        var evt = new TimecardCreatedEvent
        {
            EventId = Guid.NewGuid(),
            EventType = "TimecardCreated",
            EventVersion = 1,
            OccurredAt = DateTime.UtcNow,
            TimecardId = timecard.Id,
            WorkerId = timecard.WorkerId,
            ProjectId = timecard.ProjectId,
            WeekStart = timecard.WeekStart,
            TotalHours = timecard.TotalHours
        };

        // 3️⃣ Build Outbox entry
        var outbox = new EventOutbox
        {
            Id = Guid.NewGuid(),
            EventType = evt.EventType,
            Payload = JsonSerializer.SerializeToElement(evt),
            OccurredAt = evt.OccurredAt,
            ProcessedAt = null
        };

        // 4️⃣ SAME TRANSACTION
        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            _db.Timecards.Add(timecard);
            _db.EventOutbox.Add(outbox);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();

            // Idempotent retry handling
            var existing = await _db.Timecards.FirstAsync(t =>
                t.WorkerId == dto.WorkerId &&
                t.ProjectId == dto.ProjectId &&
                t.WeekStart == dto.WeekStart);

            return Ok(new { id = existing.Id });
        }

        return CreatedAtAction(nameof(Get), new { id = timecard.Id }, new
        {
            id = timecard.Id
        });
    }


    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusDto dto)
    {
        var tc = await _db.Timecards.FindAsync(id);
        if (tc == null) return NotFound();

        tc.Status = dto.Status;
        tc.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(tc);
    }
}