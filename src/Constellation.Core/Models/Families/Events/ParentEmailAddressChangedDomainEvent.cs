namespace Constellation.Core.Models.Families.Events;

using DomainEvents;
using Identifiers;

public sealed record ParentEmailAddressChangedDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    ParentId ParentId,
    string OldEmail,
    string NewEmail)
    : DomainEvent(Id);