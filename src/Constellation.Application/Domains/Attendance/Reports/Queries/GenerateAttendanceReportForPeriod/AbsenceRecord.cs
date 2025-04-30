namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForPeriod;

using Core.Models.Students.Identifiers;
using System;

public sealed record AbsenceRecord(
    StudentId StudentId,
    string AbsenceReason,
    DateOnly AbsenceDate,
    string AbsenceLesson,
    int Severity);