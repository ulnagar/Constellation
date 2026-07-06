namespace Constellation.Application.Domains.Assessments.Assessments.Commands.UploadSubmissionToCanvas;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;

public sealed record UploadSubmissionToCanvasCommand(
    AssessmentId AssessmentId,
    SubmissionId SubmissionId)
    : ICommand;
