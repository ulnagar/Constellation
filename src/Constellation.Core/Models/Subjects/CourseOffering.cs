namespace Constellation.Core.Models.Subjects;

using Constellation.Core.Models.Absences;
using System;
using System.Collections.Generic;

public class CourseOffering
{
    private readonly List<Resource> _resources = new();

    public CourseOffering()
    {
    }

    public CourseOffering(int courseId, DateTime startDate, DateTime endDate)
    {
        CourseId = courseId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Enrolment> Enrolments { get; set; } = new();
    public List<OfferingSession> Sessions { get; set; } = new();
    public IReadOnlyList<Resource> Resources => _resources;
    public List<Absence> Absences { get; set; } = new();
}