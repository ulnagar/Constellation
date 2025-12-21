namespace Constellation.Core.Models.EmergencyConsole.Events;

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