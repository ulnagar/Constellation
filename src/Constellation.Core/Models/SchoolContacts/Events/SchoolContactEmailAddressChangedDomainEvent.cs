namespace Constellation.Core.Models.SchoolContacts.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record SchoolContactEmailAddressChangedDomainEvent(
    DomainEventId Id,
    SchoolContactId ContactId,
    string OldEmailAddress,
    string NewEmailAddress)
    : DomainEvent(Id);