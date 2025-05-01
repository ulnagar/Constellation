namespace Constellation.Application.Domains.LinkedSystems.Canvas.Models;

using Core.Models.Canvas.Models;

public sealed record CourseEnrolmentEntry(
    int EnrollmentId,
    CanvasCourseCode CourseCode,
    CanvasSectionCode SectionCode,
    string UserId,
    int CanvasUserId,
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