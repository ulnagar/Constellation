namespace Constellation.Application.Domains.Assessments.Provisions.Commands.CreateNewAssessmentProvision;

using Abstractions.Messaging;
using Core.Models.Assessments.ValueObjects;

public sealed record CreateNewAssessmentProvisionCommand(
    ProvisionCode Code,
    string Description)
    : ICommand;
