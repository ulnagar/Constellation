#nullable enable
namespace Constellation.Core.Models.Tutorials;

using Identifiers;
using StaffMembers.Identifiers;
using System;
using System.Collections.Generic;
using Timetables.Identifiers;
using ValueObjects;

public sealed class RequestPlan
{
    private RequestPlan() { }

    public RequestPlan(
        TutorialName name,
        List<(PeriodId PeriodId, StaffId StaffId)> periods,
        DateOnly startDate)
    {
        Name = name;
        Periods = periods;
        StartDate = startDate;
    }

    public TutorialId TutorialId { get; private set; } = TutorialId.Empty;
    public TutorialName Name { get; private set; }
    public List<(PeriodId PeriodId, StaffId StaffId)> Periods { get; private set; }
    public DateOnly StartDate { get; private set; }

    public void Update(
        TutorialId tutorialId)
    {
        TutorialId = tutorialId;
    }
}