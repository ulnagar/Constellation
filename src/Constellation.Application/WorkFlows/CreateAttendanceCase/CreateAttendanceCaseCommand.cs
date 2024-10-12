namespace Constellation.Application.WorkFlows.CreateAttendanceCase;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record CreateAttendanceCaseCommand(
    StudentId StudentId)
    : ICommand;