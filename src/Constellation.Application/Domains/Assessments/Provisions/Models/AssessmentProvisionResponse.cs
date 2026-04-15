namespace Constellation.Application.Domains.Assessments.Provisions.Models;

using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.ValueObjects;
using Core.Models.Students.Identifiers;

public sealed record AssessmentProvisionResponse(
    ProvisionId Id,
    ProvisionCode Code,
    string Description);