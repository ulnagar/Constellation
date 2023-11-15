namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Core.Enums;
using Core.ValueObjects;

public sealed record AttendanceRecord(
    string StudentId,
    Name StudentName,
    Grade Grade,
    decimal Percentage,
    string Group,
    bool Improvement,
    bool Decline);