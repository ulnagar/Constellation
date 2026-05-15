namespace Constellation.Application.Domains.Assessments.Assessments.Commands.RemoveInstructionFromAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;

public sealed record RemoveInstructionFromAssessmentCommand(
    AssessmentId AssessmentId,
    AssessmentInstructionId InstructionId)
    : ICommand;
