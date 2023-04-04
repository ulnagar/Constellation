namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record ParentRemovedFromFamilyDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    string EmailAddress)
    : DomainEvent(Id);
