namespace Constellation.Application.Attendance.GetValidAttendanceReportDates;

using System;

public sealed record ValidAttendenceReportDate(
    string TermGroup,
    DateOnly StartDate,
    DateOnly EndDate,
    string Description);