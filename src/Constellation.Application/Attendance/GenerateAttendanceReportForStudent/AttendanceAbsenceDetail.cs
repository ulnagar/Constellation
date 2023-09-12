namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System;

public sealed record AttendanceAbsenceDetail(
    DateOnly Date,
    OfferingId OfferingId,
    TimeOnly StartTime,
    AbsenceType Type,
    string AbsenceTimeframe,
    AbsenceReason AbsenceReason,
    string Explanation,
    bool Explained);
