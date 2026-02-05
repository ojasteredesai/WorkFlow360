using System.Text.Json;
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

    // 👇 NOTE: JsonElement, not string, not JsonDocument
    [Column("payload", TypeName = "jsonb")]
    public JsonElement Payload { get; set; }

    [Column("occurred_at")]
    public DateTime OccurredAt { get; set; }

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }
}
