using Microsoft.EntityFrameworkCore;
using VmsService.Models;

namespace VmsService.Data;

public class VmsDbContext : DbContext
{
    public VmsDbContext(DbContextOptions<VmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<Engagement> Engagements => Set<Engagement>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Requisition> Requisitions => Set<Requisition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Schema already defined in SQL.
        // No Fluent API needed.
        base.OnModelCreating(modelBuilder);
    }
}
