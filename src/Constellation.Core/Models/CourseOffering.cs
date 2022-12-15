namespace Constellation.Core.Models;

public class CourseOffering
{
    public CourseOffering() { }

    public CourseOffering(int courseId, DateTime startDate, DateTime endDate)
    {
        CourseId = courseId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public int Id { get;  set; }
    public string Name { get;  set; } = string.Empty;
    public int CourseId { get;  set; }
    public virtual Course? Course { get;  set; }
    public DateTime StartDate { get;  set; }
    public DateTime EndDate { get;  set; }
    public List<Enrolment> Enrolments { get; set; } = new();
    public List<OfferingSession> Sessions { get; set; } = new();
    public List<ClassCover> ClassCovers { get; set; } = new();
    public List<OfferingResource> Resources { get; set; } = new();
    public List<Lesson> Lessons { get; set; } = new();
    public List<Absence> Absences { get; set; } = new();
}