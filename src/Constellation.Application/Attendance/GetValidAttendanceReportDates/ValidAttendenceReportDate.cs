namespace Constellation.Application.Attendance.GetValidAttendanceReportDates;

using System;

public sealed record ValidAttendenceReportDate(
    string TermGroup,
    DateTime StartDate,
    DateTime EndDate,
    string Description);