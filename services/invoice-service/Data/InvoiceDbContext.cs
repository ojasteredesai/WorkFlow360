using Microsoft.EntityFrameworkCore;
using InvoiceService.Models;

namespace InvoiceService.Data;

public class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options)
        : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
}
