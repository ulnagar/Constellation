namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateHistoricalDailyAttendanceReport;

using Abstractions.Messaging;
using Constellation.Core.ValueObjects;
using DTOs;

public sealed record GenerateHistoricalDailyAttendanceReportQuery(
    string Year,
    SchoolTermWeek Start,
    SchoolTermWeek End)
    : IQuery<FileDto>;