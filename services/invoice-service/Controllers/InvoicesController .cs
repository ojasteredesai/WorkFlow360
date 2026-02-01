using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceService.Data;
using InvoiceService.DTOs;
using InvoiceService.Models;

namespace InvoiceService.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceDbContext _db;

    public InvoicesController(InvoiceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Invoices.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInvoiceDto dto)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            InvoiceMonth = dto.InvoiceMonth,
            TotalAmount = dto.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        return Ok(invoice);
    }
}
