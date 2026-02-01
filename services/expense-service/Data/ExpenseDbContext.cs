using Microsoft.EntityFrameworkCore;
using ExpenseService.Models;

namespace ExpenseService.Data;

public class ExpenseDbContext : DbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options)
        : base(options) { }

    public DbSet<Expense> Expenses => Set<Expense>();
}
