namespace Constellation.Application.Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;

using Abstractions.Messaging;
using Core.Enums;

public sealed record UpdateAttendanceDataForPeriodFromSentralCommand(
    string Year,
    SchoolTerm Term,
    SchoolWeek Week)
    : ICommand;