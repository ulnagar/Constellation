namespace Constellation.Core.Models.Messaging.Sms.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record SmsMessageReceivedDomainEvent(
    DomainEventId Id,
    SmsId SmsId)
    : DomainEvent(Id);