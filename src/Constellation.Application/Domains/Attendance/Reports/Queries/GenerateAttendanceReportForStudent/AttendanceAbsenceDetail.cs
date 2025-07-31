namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForStudent;

using Constellation.Core.Models.Absences.Enums;
using System;

public sealed record AttendanceAbsenceDetail(
    DateOnly Date,
    Guid SourceId,
    TimeOnly StartTime,
    AbsenceType Type,
    string AbsenceTimeframe,
    AbsenceReason AbsenceReason,
    string Explanation,
    bool Explained);
