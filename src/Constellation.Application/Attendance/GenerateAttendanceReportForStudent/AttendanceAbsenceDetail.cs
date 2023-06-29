namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Constellation.Core.Models.Absences;
using System;

public sealed record AttendanceAbsenceDetail(
    DateOnly Date,
    int OfferingId,
    TimeOnly StartTime,
    AbsenceType Type,
    string AbsenceTimeframe,
    AbsenceReason AbsenceReason,
    string Explanation,
    bool Explained);
