namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Abstractions.Messaging;
using System.IO;

public sealed record GenerateAttendanceReportForPeriodQuery(
        string PeriodLabel)
    : IQuery<MemoryStream>;