using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimecardService.Models;

[Table("event_outbox")]
public class EventOutbox
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("payload")]
    public string Payload { get; set; } = string.Empty;

    [Column("occurred_at")]
    public DateTime OccurredAt { get; set; }

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }
}
