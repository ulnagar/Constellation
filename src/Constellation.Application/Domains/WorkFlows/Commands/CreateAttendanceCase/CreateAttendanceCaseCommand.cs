namespace Constellation.Application.Domains.WorkFlows.Commands.CreateAttendanceCase;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record CreateAttendanceCaseCommand(
    StudentId StudentId)
    : ICommand;