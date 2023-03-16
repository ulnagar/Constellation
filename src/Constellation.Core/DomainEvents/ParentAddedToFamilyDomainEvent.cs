namespace Constellation.Core.DomainEvents;

using System;

public sealed record ParentAddedToFamilyDomainEvent(
    Guid Id,
    Guid FamilyId,
    Guid ParentId)
    : DomainEvent(Id);
