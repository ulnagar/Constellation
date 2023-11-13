namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Core.Enums;

public sealed record AttendanceRecord(
    string StudentId,
    Grade Grade,
    decimal Percentage,
    string Group);