namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateAttendanceCaseDetails;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record UpdateAttendanceCaseDetailsCommand(
    StudentId StudentId)
    : ICommand;