namespace TimecardService.Events;

public abstract record EventMetadata
{
    public Guid EventId { get; init; }
    public string EventType { get; init; } = string.Empty;
    public int EventVersion { get; init; }
    public DateTime OccurredAt { get; init; }
}
