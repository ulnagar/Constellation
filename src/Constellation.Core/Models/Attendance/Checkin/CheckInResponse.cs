namespace Constellation.Core.Models.Attendance.Checkin;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Models.Identifiers;
using Offerings;
using Offerings.ValueObjects;
using Students;
using Subjects;
using System;
using ValueObjects;

public sealed class CheckInResponse
{
    private CheckInResponse() { }

    public CheckInResponse(
        Student student,
        Offering offering,
        Course course,
        DateTime submitted,
        string sentiment)
    {
        StudentId = student.Id;
        Student = student.Name;
        Grade = student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram;
        SchoolCode = student.CurrentEnrolment?.SchoolCode ?? SchoolCode.Empty;
        School = student.CurrentEnrolment?.SchoolName ?? string.Empty;

        OfferingId = offering.Id;
        Offering = offering.Name;
        CourseId = course.Id;
        Course = course.Name;

        SubmittedAt = submitted;
        Sentiment = sentiment;
    }

    public StudentId StudentId { get; private set; }
    public Name Student { get; private set; }
    public Grade Grade { get; private set; }
    public SchoolCode SchoolCode { get; private set; }
    public string School { get; private set; }
    
    public OfferingId OfferingId { get; private set; }
    public OfferingName Offering { get; private set; }
    public CourseId CourseId { get; private set; }
    public string Course { get; private set; }

    public DateTime SubmittedAt { get; private set; }
    public string Sentiment { get; private set; }
}
