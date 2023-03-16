namespace Constellation.Core.DomainEvents;

using System;

public sealed record ParentRemovedFromFamilyDomainEvent(
    Guid Id,
    Guid FamilyId,
    Guid ParentId)
    : DomainEvent(Id);
