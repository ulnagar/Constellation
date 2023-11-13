namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Core.Enums;
using System;

public sealed record AbsenceRecord(
    string StudentId,
    string StudentName,
    Grade Grade,
    string AbsenceReason,
    DateOnly AbsenceDate,
    string AbsenceLesson,
    int Severity);