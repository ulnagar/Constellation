namespace Constellation.Core.DomainEvents;

using System;

public sealed record FamilyEmailAddressChangedDomainEvent(
    Guid Id,
    Guid FamilyId,
    string OldEmail,
    string NewEmail)
    : DomainEvent(Id);
