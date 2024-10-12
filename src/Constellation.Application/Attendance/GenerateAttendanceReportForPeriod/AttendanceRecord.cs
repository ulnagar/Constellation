namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record AttendanceRecord(
    StudentId StudentId,
    Name StudentName,
    Grade Grade,
    decimal Percentage,
    string Group,
    string Improvement,
    string Decline);