namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

public sealed record AttendanceDateDetail(
    DateOnly Date,
    int DayNumber,
    List<AttendanceDateDetail.SessionWithOffering> Sessions)
{
    public sealed record SessionWithOffering(
        string PeriodName,
        string PeriodTimeframe,
        string OfferingName,
        string CourseName,
        OfferingId OfferingId);
}
