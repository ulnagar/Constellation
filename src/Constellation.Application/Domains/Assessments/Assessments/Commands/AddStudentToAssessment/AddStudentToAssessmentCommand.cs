namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddStudentToAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record AddStudentToAssessmentCommand(
    AssessmentId AssessmentId,
    StudentId StudentId)
    : ICommand;