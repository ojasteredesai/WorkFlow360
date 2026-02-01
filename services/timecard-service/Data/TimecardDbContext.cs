using Microsoft.EntityFrameworkCore;
using TimecardService.Models;

namespace TimecardService.Data;

public class TimecardDbContext : DbContext
{
    public TimecardDbContext(DbContextOptions<TimecardDbContext> options)
        : base(options) { }

    public DbSet<Timecard> Timecards => Set<Timecard>();
}
