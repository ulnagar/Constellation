namespace Constellation.Application.Attendance.GenerateHistoricalDailyAttendanceReport;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using System;
using System.IO;

public sealed record GenerateHistoricalDailyAttendanceReportCommand(
    string Year,
    SchoolTermWeek Start,
    SchoolTermWeek End)
    : ICommand<MemoryStream>;