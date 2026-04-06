namespace Constellation.Core.Models.Messaging.Tracking;

using Email.Identifiers;
using Identifiers;
using System;

public abstract record TrackingEvent
{
    public TrackingEventId Id { get; init; } = new();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public sealed record EmailOpenEvent(EmailId EmailId) : TrackingEvent;

public sealed record SmsDeliveryReceiptEvent(
    string OutgoingId,
    string? Status,
    DateTimeOffset DateTime)
    : TrackingEvent;

public sealed record EmailClickEvent(
    EmailId EmailId,
    string DestinationUrl)
    : TrackingEvent;