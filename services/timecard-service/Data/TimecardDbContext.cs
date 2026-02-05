using Microsoft.EntityFrameworkCore;
using TimecardService.Models;

namespace TimecardService.Data;

public class TimecardDbContext : DbContext
{
    public TimecardDbContext(DbContextOptions<TimecardDbContext> options)
        : base(options) { }

    public DbSet<Timecard> Timecards => Set<Timecard>();
    public DbSet<EventOutbox> EventOutbox => Set<EventOutbox>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Timecard>()
            .HasIndex(t => new { t.WorkerId, t.ProjectId, t.WeekStart })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
