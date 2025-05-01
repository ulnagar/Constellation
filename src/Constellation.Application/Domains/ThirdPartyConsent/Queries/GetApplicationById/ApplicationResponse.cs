namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetApplicationById;

using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record ApplicationResponse(
    ApplicationId Id,
    string Name,
    string Purpose,
    string[] InformationCollected,
    string StoredCountry,
    string[] SharedWith,
    bool ConsentRequired);