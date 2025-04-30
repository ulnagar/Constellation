namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendanceDataForYearFromSentral;

using Abstractions.Messaging;

public sealed record GetAttendanceDataForYearFromSentralCommand()
    : ICommand;