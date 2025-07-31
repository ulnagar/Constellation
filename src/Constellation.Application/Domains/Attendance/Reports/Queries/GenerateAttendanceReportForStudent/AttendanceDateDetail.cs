namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForStudent;

using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

public sealed record AttendanceDateDetail(
    DateOnly Date,
    int DayNumber,
    List<AttendanceDateDetail.SessionWithSource> Sessions)
{
    public sealed record SessionWithSource(
        string PeriodName,
        string PeriodTimeframe,
        string OfferingName,
        string CourseName,
        Guid SourceId);
}
