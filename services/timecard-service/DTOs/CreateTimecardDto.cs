namespace TimecardService.DTOs;

public class CreateTimecardDto
{
    public Guid WorkerId { get; set; }
    public Guid ProjectId { get; set; }
    public DateOnly WeekStart { get; set; }
    public int TotalHours { get; set; }
}