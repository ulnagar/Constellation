namespace Constellation.Core.Models.SchoolContacts.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record SchoolContactCreatedDomainEvent(
    DomainEventId Id,
    SchoolContactId ContactId)
    : DomainEvent(Id);