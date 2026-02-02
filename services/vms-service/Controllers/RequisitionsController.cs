using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VmsService.Data;
using VmsService.Models;

namespace VmsService.Controllers;

[ApiController]
[Route("api/requisitions")]
public class RequisitionsController : ControllerBase
{
    private readonly VmsDbContext _db;

    public RequisitionsController(VmsDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Requisitions.ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var requisition = await _db.Requisitions.FindAsync(id);
        return requisition == null ? NotFound() : Ok(requisition);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Requisition requisition)
    {
        requisition.Id = Guid.NewGuid();
        requisition.CreatedAt = DateTime.UtcNow;

        _db.Requisitions.Add(requisition);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = requisition.Id }, requisition);
    }
}
