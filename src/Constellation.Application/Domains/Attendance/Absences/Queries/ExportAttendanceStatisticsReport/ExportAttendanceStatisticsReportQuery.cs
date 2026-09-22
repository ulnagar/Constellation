namespace Constellation.Application.Domains.Attendance.Absences.Queries.ExportAttendanceStatisticsReport;

using Abstractions.Messaging;
using DTOs;

public sealed class ExportAttendanceStatisticsReportQuery
    : IQuery<byte[]>;