namespace Constellation.Application.Domains.Assessments.Provisions.Commands.AssignProvisionToStudent;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record AssignProvisionToStudentCommand(
    StudentId StudentId,
    ProvisionId ProvisionId)
    : ICommand;
