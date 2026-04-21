namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddProvisionToAssessmentStudent;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record AddProvisionToAssessmentStudentCommand(
    AssessmentId AssessmentId,
    StudentId StudentId,
    List<ProvisionId> ProvisionIds)
    : ICommand;