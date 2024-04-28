namespace Constellation.Application.WorkFlows.UpdateAttendanceCaseDetails;

using Abstractions.Messaging;

public sealed record UpdateAttendanceCaseDetailsCommand(
    string StudentId)
    : ICommand;