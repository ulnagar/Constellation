namespace Constellation.Core.Models.Offerings.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record AllSessionsDeletedDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    List<string> StaffIds,
    List<string> RoomIds)
    : DomainEvent(Id);