namespace Constellation.Core.Models.Messaging.Tracking;

using Identifiers;
using System;

public sealed class TrackingQueueEntry
{
    public TrackingQueueEntryId Id { get; set; } = new();
    public required string EventType { get; set; }
    public required string Payload { get; set; }
    public DateTimeOffset EnqueuedAt { get; set; } = DateTimeOffset.UtcNow;
    public int Attempts { get; set; } = 0;
    public DateTimeOffset? RetryAfter { get; set; }
    public string? LastError { get; set; }
}
