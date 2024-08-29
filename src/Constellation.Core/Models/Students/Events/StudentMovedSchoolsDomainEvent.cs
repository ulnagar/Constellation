namespace Constellation.Core.Models.Students.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record StudentMovedSchoolsDomainEvent(
    DomainEventId Id,
    StudentId StudentId,
    string PreviousSchoolCode,
    string CurrentSchoolCode)
    : DomainEvent(Id);