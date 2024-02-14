namespace Constellation.Core.Models.Students.Events;

using DomainEvents;
using Identifiers;

public sealed record StudentMovedSchoolsDomainEvent(
    DomainEventId Id,
    string StudentId,
    string PreviousSchoolCode,
    string CurrentSchoolCode)
    : DomainEvent(Id);