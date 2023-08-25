namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Enrolment;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;

public class Offering
{
    private readonly List<Resource> _resources = new();

    public Offering()
    {
    }

    public Offering(int courseId, DateOnly startDate, DateOnly endDate)
    {
        CourseId = courseId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public OfferingId Id { get; set; }
    public string Name { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<Enrolment> Enrolments { get; set; } = new();
    public List<Session> Sessions { get; set; } = new();
    public IReadOnlyList<Resource> Resources => _resources;
    public List<Absence> Absences { get; set; } = new();
    public bool IsCurrent => IsOfferingCurrent();

    private bool IsOfferingCurrent()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        if (Sessions.All(s => s.IsDeleted))
            return false;

        if (StartDate <= today && EndDate >= today)
            return true;

        return false;
    }
}