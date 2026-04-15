namespace Constellation.Application.Domains.Assessments.Provisions.Commands.UpdateAssessmentProvision;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.ValueObjects;

public sealed record UpdateAssessmentProvisionCommand(
    ProvisionId Id,
    ProvisionCode Code,
    string Description)
    : ICommand;