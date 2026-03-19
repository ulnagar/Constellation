namespace Constellation.Core.Models.Students.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;
using System;

public sealed record StudentMovedSchoolsDomainEvent(
    DomainEventId Id,
    StudentId StudentId,
    SchoolCode PreviousSchoolCode,
    SchoolCode CurrentSchoolCode,
    DateOnly? DelayUntil = null)
    : DomainEvent(Id, DelayUntil);