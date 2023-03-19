namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record FamilyEmailAddressChangedDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    string OldEmail,
    string NewEmail)
    : DomainEvent(Id);
