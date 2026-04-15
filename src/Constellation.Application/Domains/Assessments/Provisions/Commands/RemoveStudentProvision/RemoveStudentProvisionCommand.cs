namespace Constellation.Application.Domains.Assessments.Provisions.Commands.RemoveStudentProvision;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;

public sealed record RemoveStudentProvisionCommand(
    StudentProvisionId Id)
    : ICommand;