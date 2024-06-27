namespace Constellation.Core.Models.WorkFlow.Errors;

using Constellation.Core.Shared;

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

    public static readonly Error CreateTeacherNull = new(
        "Case.Detail.Create.TeacherNull",
        "The teacher cannot be empty or null");

    public static readonly Error CreateSchoolNull = new(
        "Case.Detail.Create.SchoolNull",
        "The school cannot be empty or null");

    public static readonly Error CreateTrainingModuleNull = new(
        "Case.Detail.Create.TrainingModuleNull",
        "The training module cannot be empty or null");
}