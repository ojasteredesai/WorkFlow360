using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VmsService.Data;
using VmsService.Models;

namespace VmsService.Controllers;

[ApiController]
[Route("api/engagements")]
public class EngagementsController : ControllerBase
{
    private readonly VmsDbContext _db;

    public EngagementsController(VmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Engagements.ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var engagement = await _db.Engagements.FindAsync(id);
        return engagement == null ? NotFound() : Ok(engagement);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Engagement engagement)
    {
        engagement.Id = Guid.NewGuid();
        engagement.CreatedAt = DateTime.UtcNow;

        _db.Engagements.Add(engagement);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = engagement.Id }, engagement);
    }
}
