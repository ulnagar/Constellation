namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetValidAttendenceReportDatesQuery()
    : IQuery<List<ValidAttendenceReportDate>>;