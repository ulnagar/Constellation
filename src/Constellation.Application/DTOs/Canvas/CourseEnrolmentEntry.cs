namespace Constellation.Application.DTOs.Canvas;

using Core.Models.Canvas.Models;

public sealed record CourseEnrolmentEntry(
    int EnrollmentId,
    CanvasCourseCode CourseCode,
    CanvasSectionCode SectionCode,
    string UserId,
    CourseEnrolmentEntry.UserType Type,
    CourseEnrolmentEntry.EnrolmentRole Role)
{
    public enum UserType
    {
        Unknown,
        Student,
        Teacher
    }

    public enum EnrolmentRole
    {
        Unknown,
        Student,
        Teacher
    }
}
