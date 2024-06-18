namespace Constellation.Core.Models.WorkFlow;

using Attendance;
using Constellation.Core.Enums;
using Constellation.Core.Models.Attendance.Identifiers;
using Enums;
using Errors;
using Extensions;
using Identifiers;
using Shared;
using Students;

public abstract class CaseDetail
{
    public CaseDetailId Id { get; private protected set; } = new();
    public CaseId CaseId { get; private protected set; } = CaseId.Empty;

    public abstract override string ToString();
}

public sealed class AttendanceCaseDetail : CaseDetail
{
    private AttendanceCaseDetail() { }

    public string StudentId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Grade Grade { get; private set; }
    public string SchoolCode { get; private set; } = string.Empty;
    public string SchoolName { get; private set; } = string.Empty;
    public AttendanceValueId AttendanceValueId { get; private set; } = AttendanceValueId.Empty;
    public string PeriodLabel { get; private set; } = string.Empty;
    public AttendanceSeverity Severity { get; private set; } = AttendanceSeverity.BandZero;
    public decimal PerMinuteYearToDatePercentage { get; private set; }
    public decimal PerMinuteWeekPercentage { get; private set; }
    public decimal PerDayYearToDatePercentage { get; private set; }
    public decimal PerDayWeekPercentage { get; private set; }

    public static Result<CaseDetail> Create(
        Student? student,
        AttendanceValue? value)
    {
        if (student is null)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateStudentNull);

        if (value is null)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateAttendanceValueNull);

        if (student.StudentId != value.StudentId)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateStudentMismatch);

        AttendanceCaseDetail detail = new();
        detail.AddFromStudent(student);
        detail.AddFromAttendanceValue(value);
        detail.Severity = AttendanceSeverity.FromAttendanceValue(value.PerMinuteWeekPercentage);

        return detail;
    }

    private void AddFromAttendanceValue(AttendanceValue value)
    {
        AttendanceValueId = value.Id;
        PeriodLabel = value.PeriodLabel;
        PerMinuteYearToDatePercentage = value.PerMinuteYearToDatePercentage;
        PerMinuteWeekPercentage = value.PerMinuteWeekPercentage;
        PerDayYearToDatePercentage = value.PerDayYearToDatePercentage;
        PerDayWeekPercentage = value.PerDayWeekPercentage;
    }

    private void AddFromStudent(Student student)
    {
        StudentId = student.StudentId;
        Name = student.GetName().DisplayName;
        Grade = student.CurrentGrade;
        SchoolCode = student.SchoolCode;
        SchoolName = student.School.Name;
    }

    public override string ToString() => $"Attendance Case for {Name} ({Grade.AsName()}): {PeriodLabel} - {Severity.Name}";
}

public sealed class NAwardCaseDetail
{

}