using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimecardService.Models;

[Table("timecards")]
public class Timecard
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("worker_id")]
    public Guid WorkerId { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [Column("week_start")]
    public DateOnly WeekStart { get; set; }

    [Column("total_hours")]
    public int TotalHours { get; set; }

    [Column("status")]
    public string Status { get; set; } = "CREATED";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}