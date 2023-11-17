namespace Constellation.Application.Attendance.GetRecentAttendanceValues;

using Core.Enums;
using Core.ValueObjects;

public sealed record AttendanceValueResponse(
    string StudentId,
    Name? StudentName,
    Grade StudentGrade,
    string SchoolName,
    string PeriodLabel,
    decimal PerDayYearToDatePercentage,
    decimal PerDayWeekPercentage,
    decimal PerMinuteYearToDatePercentage,
    decimal PerMinuteWeekPercentage);