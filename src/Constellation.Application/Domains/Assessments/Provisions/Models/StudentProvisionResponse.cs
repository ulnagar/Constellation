namespace Constellation.Application.Domains.Assessments.Provisions.Models;

using Core.Models.Assessments.Identifiers;
using Core.Models.Assessments.ValueObjects;
using Core.ValueObjects;

public sealed record StudentProvisionResponse(
    StudentProvisionId Id,
    ProvisionCode Code,
    string Description,
    Name Student,
    int Year,
    bool IsDeleted);