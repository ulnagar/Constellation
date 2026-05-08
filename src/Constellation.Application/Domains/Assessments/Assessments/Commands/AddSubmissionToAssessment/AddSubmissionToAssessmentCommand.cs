namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddSubmissionToAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;
using DTOs;

public sealed record AddSubmissionToAssessmentCommand(
    AssessmentId AssessmentId,
    StudentId StudentId,
    FileDto File)
: ICommand;
