using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseService.Data;
using ExpenseService.DTOs;
using ExpenseService.Models;

namespace ExpenseService.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseDbContext _db;

    public ExpensesController(ExpenseDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Expenses.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateExpenseDto dto)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            WorkerId = dto.WorkerId,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate,
            Description = dto.Description,
            Status = "SUBMITTED",
            CreatedAt = DateTime.UtcNow
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return Ok(expense);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateExpenseStatusDto dto)
    {
        var exp = await _db.Expenses.FindAsync(id);
        if (exp == null) return NotFound();

        exp.Status = dto.Status;
        await _db.SaveChangesAsync();

        return Ok(exp);
    }
}