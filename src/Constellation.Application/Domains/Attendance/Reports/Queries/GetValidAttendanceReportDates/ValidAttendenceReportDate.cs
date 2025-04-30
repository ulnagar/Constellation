namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;

using System;

public sealed record ValidAttendenceReportDate(
    string TermGroup,
    DateTime StartDate,
    DateTime EndDate,
    string Description);