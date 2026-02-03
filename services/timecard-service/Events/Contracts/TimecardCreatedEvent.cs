namespace TimecardService.Events.Contracts;

/// <summary>
/// Emitted after a timecard is successfully persisted.
/// Immutable, versionable, and replay-safe.
/// </summary>
public record TimecardCreatedEvent : EventMetadata
{
    public Guid TimecardId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ProjectId { get; init; }
    public DateOnly WeekStart { get; init; }
    public int TotalHours { get; init; }
}


