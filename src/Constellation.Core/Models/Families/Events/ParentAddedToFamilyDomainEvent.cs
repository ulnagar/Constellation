namespace Constellation.Core.Models.Families.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;

public sealed record ParentAddedToFamilyDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    ParentId ParentId)
    : DomainEvent(Id);
