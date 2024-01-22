namespace Constellation.Application.ThirdPartyConsent.GetApplications;

using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record ApplicationSummaryResponse(
    ApplicationId Id,
    string Name,
    string Purpose,
    bool ConsentRequired,
    bool IsDeleted,
    int CountResponses);
