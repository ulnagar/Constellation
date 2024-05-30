using Constellation.Core.Shared;

namespace Constellation.Core.Models.WorkFlow.Errors;

public static class CaseDetailErrors
{
    public static readonly Error CreateStudentNull = new(
        "Case.Detail.Create.StudentNull",
        "The student cannot be empty or null");

    public static readonly Error CreateAttendanceValueNull = new(
        "Case.Detail.Create.AttendanceValueNull",
        "The Attendance Value cannot be empty or null");

    public static readonly Error CreateStudentMismatch = new(
        "Case.Detail.Create.StudentMismatch",
        "The Attendance Value is not linked to the Student");
}