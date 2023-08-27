namespace Constellation.Core.Models.Subjects.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record AllSessionsDeletedDomainEvent(
    DomainEventId Id,
    OfferingId OfferingId,
    List<string> StaffIds,
    List<string> RoomIds)
    : DomainEvent(Id);