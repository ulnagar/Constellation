namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record ParentEmailAddressChangedDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    ParentId ParentId,
    string OldEmail,
    string NewEmail)
    : DomainEvent(Id);