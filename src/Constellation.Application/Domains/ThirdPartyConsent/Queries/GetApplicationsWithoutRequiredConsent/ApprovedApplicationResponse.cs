namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetApplicationsWithoutRequiredConsent;

using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record ApprovedApplicationResponse(
    ApplicationId Id,
    string Name,
    string Purpose,
    string Link);