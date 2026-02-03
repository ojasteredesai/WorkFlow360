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
        var tc = new Timecard
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

        _db.Timecards.Add(tc);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            tc = await _db.Timecards.FirstAsync(t =>
                t.WorkerId == dto.WorkerId &&
                t.ProjectId == dto.ProjectId &&
                t.WeekStart == dto.WeekStart);
        }

        // 🔑 EVENT PUBLISH HAPPENS *AFTER* DB COMMIT
        var evt = new TimecardCreatedEvent
        {
            EventId = Guid.NewGuid(),
            TimecardId = tc.Id,
            WorkerId = tc.WorkerId,
            ProjectId = tc.ProjectId,
            WeekStart = tc.WeekStart,
            TotalHours = tc.TotalHours,
            OccurredAt = DateTime.UtcNow
        };

        await _eventPublisher.PublishAsync(
            routingKey: "timecard.created",
            message: evt);

        return Ok(new { id = tc.Id });
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