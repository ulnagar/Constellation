namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForPeriod;

using Abstractions.Messaging;
using System.IO;

public sealed record GenerateAttendanceReportForPeriodQuery(
        string PeriodLabel)
    : IQuery<MemoryStream>;