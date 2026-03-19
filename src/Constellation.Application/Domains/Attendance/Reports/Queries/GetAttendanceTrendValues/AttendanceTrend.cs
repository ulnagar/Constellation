namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendanceTrendValues;

using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.WorkFlow.Enums;
using Core.ValueObjects;

public sealed record AttendanceTrend(
    StudentId StudentId,
    Name Name,
    Grade Grade,
    SchoolCode SchoolCode,
    string SchoolName,
    string PeriodName,
    bool ExistingCase,
    decimal WeekZeroValue,
    decimal WeekOneValue,
    decimal WeekTwoValue,
    decimal WeekThreeValue,
    decimal WeekFourValue,
    AttendanceSeverity Severity);