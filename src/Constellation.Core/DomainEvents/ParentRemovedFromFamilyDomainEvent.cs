namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record ParentRemovedFromFamilyDomainEvent(
    DomainEventId Id,
    FamilyId FamilyId,
    ParentId ParentId)
    : DomainEvent(Id);
