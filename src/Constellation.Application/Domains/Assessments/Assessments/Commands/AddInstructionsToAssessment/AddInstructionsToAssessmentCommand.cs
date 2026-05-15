namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddInstructionsToAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments.Enums;
using Core.Models.Assessments.Identifiers;

public sealed record AddInstructionsToAssessmentCommand(
    AssessmentId AssessmentId,
    UserCategory Category,
    string Instructions)
    : ICommand;