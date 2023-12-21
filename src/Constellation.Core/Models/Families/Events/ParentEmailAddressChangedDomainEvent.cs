namespace Constellation.Core.Models.Families.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;

public sealed record ParentEmailAddressChangedDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    ParentId ParentId,
    string OldEmail,
    string NewEmail)
    : DomainEvent(Id);