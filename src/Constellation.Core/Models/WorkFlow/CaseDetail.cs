namespace Constellation.Core.Models.WorkFlow;

using Constellation.Core.Enums;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Identifiers;
using Errors;
using Identifiers;
using Shared;
using Students;

public abstract class CaseDetail
{
    public CaseDetailId Id { get; private protected set; } = new();
    public CaseId CaseId { get; private protected set; }
}

public sealed class AttendanceCaseDetail : CaseDetail
{
    private AttendanceCaseDetail() { }

    public string StudentId { get; private set; }
    public string Name { get; private set; }
    public Grade Grade { get; private set; }
    public string SchoolCode { get; private set; }
    public string SchoolName { get; private set; }
    public AttendanceValueId AttendanceValueId { get; private set; }
    public string PeriodLabel { get; private set; }
    public decimal PerMinuteYearToDatePercentage { get; private set; }
    public decimal PerMinuteWeekPercentage { get; private set; }
    public decimal PerDayYearToDatePercentage { get; private set; }
    public decimal PerDayWeekPercentage { get; private set; }

    public static Result<CaseDetail> Create(
        Student student,
        AttendanceValue value)
    {
        if (student is null)
            return Result.Failure<CaseDetail>(CaseErrors.CaseDetail.Create.StudentNull);

        if (value is null)
            return Result.Failure<CaseDetail>(CaseErrors.CaseDetail.Create.AttendanceValueNull);

        if (student.StudentId != value.StudentId)
            return Result.Failure<CaseDetail>(CaseErrors.CaseDetail.Create.StudentMismatch);

        AttendanceCaseDetail detail = new();
        detail.AddFromStudent(student);
        detail.AddFromAttendanceValue(value);

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
}