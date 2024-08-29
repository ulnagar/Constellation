namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Core.Models.Students.Identifiers;
using System;

public sealed record AbsenceRecord(
    StudentId StudentId,
    string AbsenceReason,
    DateOnly AbsenceDate,
    string AbsenceLesson,
    int Severity);