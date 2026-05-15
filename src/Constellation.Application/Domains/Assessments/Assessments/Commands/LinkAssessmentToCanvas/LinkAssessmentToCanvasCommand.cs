namespace Constellation.Application.Domains.Assessments.Assessments.Commands.LinkAssessmentToCanvas;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Canvas.Models;

public sealed record LinkAssessmentToCanvasCommand(
    AssessmentId AssessmentId,
    CanvasCourseCode CanvasCourse,
    int CanvasAssignmentId,
    int AllowedAttempts,
    DateTimeOffset ForwardDate)
    : ICommand;