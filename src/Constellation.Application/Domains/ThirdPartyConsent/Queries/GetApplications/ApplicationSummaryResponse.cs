namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetApplications;

using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record ApplicationSummaryResponse(
    ApplicationId Id,
    string Name,
    string Purpose,
    bool ConsentRequired,
    bool IsDeleted,
    int CountResponses);
