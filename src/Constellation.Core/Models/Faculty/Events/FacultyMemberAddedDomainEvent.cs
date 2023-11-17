namespace Constellation.Core.Models.Faculty.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Faculty.Identifiers;
using Constellation.Core.Models.Identifiers;

public sealed record FacultyMemberAddedDomainEvent(
    DomainEventId Id,
    FacultyId FacultyId,
    FacultyMembershipId FacultyMembershipId)
    : DomainEvent(Id);