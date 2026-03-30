namespace Constellation.Core.Models.Messaging.Drafts.Events;

using DomainEvents;
using Enums;
using Models.Identifiers;
using ValueObjects;

public sealed record MessageDraftMarkedForSendingDomainEvent(
    DomainEventId Id,
    MessageType Type,
    MessageSender Sender,
    string? Subject,
    string Body,
    IReadOnlyList<MessageDraftMarkedForSendingDomainEvent.RecipientSnapshot> Recipients)
    : DomainEvent(Id)
{
    public sealed record RecipientSnapshot
    {
        public string Name { get; init; } = string.Empty;
        public string? EmailAddress { get; init; }
        public string? PhoneNumber { get; init; }
    }
};