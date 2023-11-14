namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using System;

public sealed record AbsenceRecord(
    string StudentId,
    string AbsenceReason,
    DateOnly AbsenceDate,
    string AbsenceLesson,
    int Severity);