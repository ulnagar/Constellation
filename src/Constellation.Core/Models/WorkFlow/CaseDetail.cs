﻿namespace Constellation.Core.Models.WorkFlow;

using Abstractions.Clock;
using Attendance;
using Constellation.Core.Enums;
using Constellation.Core.Errors;
using Constellation.Core.Models.Attendance.Identifiers;
using Constellation.Core.Models.Training.Identifiers;
using Enums;
using Errors;
using Extensions;
using Identifiers;
using Shared;
using Students;
using System;
using Training;

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

public sealed class ComplianceCaseDetail : CaseDetail
{
    public string StudentId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Grade Grade { get; private set; }
    public string SchoolCode { get; private set; } = string.Empty;
    public string SchoolName { get; private set; } = string.Empty;
    public string IncidentId { get; private set; } = string.Empty;
    public string IncidentType { get; private set; } = string.Empty;
    public DateOnly CreatedDate { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string CreatedById { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;

    public static Result<CaseDetail> Create(
        Student student,
        School school,
        Staff teacher,
        string incidentId,
        string incidentType,
        string subject,
        DateOnly createdDate)
    {
        if (student is null)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateStudentNull);

        if (school is null)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateSchoolNull);

        if (teacher is null)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateTeacherNull);

        if (string.IsNullOrWhiteSpace(incidentId))
            return Result.Failure<CaseDetail>(ApplicationErrors.ArgumentNull(nameof(incidentId)));

        if (string.IsNullOrWhiteSpace(incidentType))
            return Result.Failure<CaseDetail>(ApplicationErrors.ArgumentNull(nameof(incidentType)));

        if (string.IsNullOrWhiteSpace(subject))
            return Result.Failure<CaseDetail>(ApplicationErrors.ArgumentNull(nameof(subject)));
        
        if (createdDate == DateOnly.MinValue)
            return Result.Failure<CaseDetail>(ApplicationErrors.ArgumentNull(nameof(createdDate)));

        ComplianceCaseDetail detail = new()
        {
            StudentId = student.StudentId,
            Name = student.GetName().DisplayName, 
            Grade = student.CurrentGrade,
            SchoolCode = student.SchoolCode,
            SchoolName = school.Name,
            IncidentId = incidentId,
            IncidentType = incidentType,
            Subject = subject,
            CreatedDate = createdDate,
            CreatedById = teacher.StaffId,
            CreatedBy = teacher.GetName().DisplayName
        };

        return detail;
    }

    public override string ToString() => $"Compliance Case for {Name} ({Grade.AsName()}): {IncidentType} - {Subject}";
}

public sealed class TrainingCaseDetail : CaseDetail
{
    public string StaffId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public TrainingModuleId TrainingModuleId { get; private set;}
    public string ModuleName { get; private set; } = string.Empty;
    public DateOnly DueDate { get; private set; }
    public int DaysUntilDue => (int)DueDate.ToDateTime(TimeOnly.MinValue).Subtract(DateTime.Today).TotalDays;

    public static Result<CaseDetail> Create(
        Staff teacher,
        TrainingModule module,
        TrainingCompletion? completion,
        IDateTimeProvider dateTime)
    {
        if (teacher is null)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateTeacherNull);

        if (module is null)
            return Result.Failure<CaseDetail>(CaseDetailErrors.CreateTrainingModuleNull);

        DateOnly dueDate = (completion is null)
            ? dateTime.Today
            : completion.CompletedDate.AddYears((int)module.Expiry);

        TrainingCaseDetail detail = new()
        {
            StaffId = teacher.StaffId,
            Name = teacher.GetName().DisplayName,
            TrainingModuleId = module.Id,
            ModuleName = module.Name,
            DueDate = dueDate
        };

        return detail;
    }

    public override string ToString() => $"Training Case for {Name}: {ModuleName} - {DueDate:d}";
}