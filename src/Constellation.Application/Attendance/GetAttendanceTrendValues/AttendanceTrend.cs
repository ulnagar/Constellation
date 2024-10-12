namespace Constellation.Application.Attendance.GetAttendanceTrendValues;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.WorkFlow.Enums;
using Core.ValueObjects;

public sealed record AttendanceTrend(
    StudentId StudentId,
    Name Name,
    Grade Grade,
    string SchoolCode,
    string SchoolName,
    string PeriodName,
    bool ExistingCase,
    decimal WeekZeroValue,
    decimal WeekOneValue,
    decimal WeekTwoValue,
    decimal WeekThreeValue,
    decimal WeekFourValue,
    AttendanceSeverity Severity);