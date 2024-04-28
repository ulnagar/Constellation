namespace Constellation.Application.WorkFlows.CreateAttendanceCase;

using Abstractions.Messaging;

public sealed record CreateAttendanceCaseCommand(
    string StudentId)
    : ICommand;