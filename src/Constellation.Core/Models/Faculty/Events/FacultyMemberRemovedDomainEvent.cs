namespace Constellation.Core.Models.Faculty.Events;

using DomainEvents;
using Identifiers;
using Constellation.Core.Models.Identifiers;

public sealed record FacultyMemberRemovedDomainEvent(
    DomainEventId Id,
    FacultyId FacultyId,
    FacultyMembershipId FacultyMembershipId)
    : DomainEvent(Id);