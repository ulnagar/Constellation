namespace Constellation.Application.DTOs.Canvas;
public sealed record CourseEnrolmentEntry(
    string CourseCode,
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
