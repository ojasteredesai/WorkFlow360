using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceService.Data;
using InvoiceService.DTOs;
using InvoiceService.Models;

namespace InvoiceService.Controllers;

[ApiController]
[Route("api/payroll")]
public class PayrollController : ControllerBase
{
    private readonly InvoiceDbContext _db;

    public PayrollController(InvoiceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Payrolls.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePayrollDto dto)
    {
        var payroll = new Payroll
        {
            Id = Guid.NewGuid(),
            WorkerId = dto.WorkerId,
            PayrollMonth = dto.PayrollMonth,
            Amount = dto.Amount,
            CreatedAt = DateTime.UtcNow
        };

        _db.Payrolls.Add(payroll);
        await _db.SaveChangesAsync();

        return Ok(payroll);
    }
}
