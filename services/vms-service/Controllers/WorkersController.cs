using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VmsService.Data;
using VmsService.Models;

namespace VmsService.Controllers;

[ApiController]
[Route("api/workers")]
public class WorkersController : ControllerBase
{
    private readonly VmsDbContext _db;

    public WorkersController(VmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Workers.ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var worker = await _db.Workers.FindAsync(id);
        return worker == null ? NotFound() : Ok(worker);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Worker worker)
    {
        worker.Id = Guid.NewGuid();
        worker.CreatedAt = DateTime.UtcNow;

        _db.Workers.Add(worker);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = worker.Id }, worker);
    }
}
