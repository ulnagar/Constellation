namespace Constellation.Core.Models.Tutorials.Events;

using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Timetables.Identifiers;
using Constellation.Core.Models.Tutorials.ValueObjects;
using DomainEvents;
using Identifiers;
using Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record TutorialRequestScheduledDomainEvent(
    DomainEventId Id,
    RequestId RequestId,
    TutorialName Name,
    List<(PeriodId PeriodId, StaffId StaffId)> Periods,
    DateOnly StartDate)
    : DomainEvent(Id);