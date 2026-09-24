namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportAttendanceStatisticsReport;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Absences.Models;

public sealed record ExportAttendanceStatisticsReportQuery(
    AttendanceStatisticsResponse Statistics)
    : IQuery<byte[]>;