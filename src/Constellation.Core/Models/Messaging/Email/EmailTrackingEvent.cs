namespace Constellation.Core.Models.Messaging.Email;

using Enums;
using Identifiers;

public sealed class EmailTrackingEvent
{
    public required EmailTrackingEventId Id { get; set; }
    public EmailId EmailId { get; set; }
    public EmailEventType EventType { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? LinkUrl { get; set; }    // populated for Click events
    public string? Metadata { get; set; }   // JSON, for any provider-specific data
}