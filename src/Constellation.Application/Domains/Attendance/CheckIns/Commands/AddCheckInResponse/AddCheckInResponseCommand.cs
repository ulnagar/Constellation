namespace Constellation.Application.Domains.Attendance.CheckIns.Commands.AddCheckInResponse;

using Abstractions.Messaging;
using Models;

public sealed record AddCheckInResponseCommand(
    FormResponse Response)
    : ICommand;