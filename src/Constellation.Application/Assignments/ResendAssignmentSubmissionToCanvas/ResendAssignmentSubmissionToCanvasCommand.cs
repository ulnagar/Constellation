namespace Constellation.Application.Assignments.ResendAssignmentSubmissionToCanvas;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assignments.Identifiers;

public sealed record ResendAssignmentSubmissionToCanvasCommand(
    AssignmentId AssignmentId,
    AssignmentSubmissionId SubmissionId)
    : ICommand;
