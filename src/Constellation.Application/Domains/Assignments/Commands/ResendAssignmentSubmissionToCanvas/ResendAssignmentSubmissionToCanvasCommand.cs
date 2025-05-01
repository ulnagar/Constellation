namespace Constellation.Application.Domains.Assignments.Commands.ResendAssignmentSubmissionToCanvas;

using Abstractions.Messaging;
using Core.Models.Assignments.Identifiers;

public sealed record ResendAssignmentSubmissionToCanvasCommand(
    AssignmentId AssignmentId,
    AssignmentSubmissionId SubmissionId)
    : ICommand;
