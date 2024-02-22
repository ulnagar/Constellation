namespace Constellation.Core.Models.Families.Events;

using DomainEvents;
using Identifiers;

public sealed record FamilyEmailAddressChangedDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    string OldEmail,
    string NewEmail)
    : DomainEvent(Id);
