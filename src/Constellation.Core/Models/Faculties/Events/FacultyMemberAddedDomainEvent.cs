namespace Constellation.Core.Models.Faculties.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Faculties.Identifiers;
using Constellation.Core.Models.Identifiers;

public sealed record FacultyMemberAddedDomainEvent(
    DomainEventId Id,
    FacultyId FacultyId,
    FacultyMembershipId FacultyMembershipId)
    : DomainEvent(Id);