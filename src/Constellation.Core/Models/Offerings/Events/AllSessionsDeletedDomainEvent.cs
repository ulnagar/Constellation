namespace Constellation.Core.Models.Offerings.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;
using System.Collections.Generic;

public sealed record AllSessionsDeletedDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    IList<string> StaffIds,
    IList<string> RoomIds)
    : DomainEvent(Id);