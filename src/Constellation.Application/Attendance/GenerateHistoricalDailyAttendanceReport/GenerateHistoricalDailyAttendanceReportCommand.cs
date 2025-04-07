namespace Constellation.Application.Attendance.GenerateHistoricalDailyAttendanceReport;

using Abstractions.Messaging;
using System;
using System.IO;

public sealed record GenerateHistoricalDailyAttendanceReportCommand(
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<MemoryStream>;