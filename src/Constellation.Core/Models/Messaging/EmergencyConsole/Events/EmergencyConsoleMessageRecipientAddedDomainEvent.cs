namespace Constellation.Core.Models.Messaging.EmergencyConsole.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;
using ValueObjects;

public sealed record EmergencyConsoleMessageRecipientAddedDomainEvent(
    DomainEventId Id,
    EventId EventId,
    MessageId MessageId,
    AlertRecipient Recipient)
    : DomainEvent(Id);