namespace Constellation.Application.Domains.Assessments.Assessments.Commands.RemoveStudentFromAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record RemoveStudentFromAssessmentCommand(
    AssessmentId AssessmentId,
    StudentId StudentId)
    : ICommand;
