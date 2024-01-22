namespace Constellation.Application.ThirdPartyConsent.GetApplicationById;

using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record ApplicationResponse(
    ApplicationId Id,
    string Name,
    string Purpose,
    string[] InformationCollected,
    string StoredCountry,
    string[] SharedWith,
    bool ConsentRequired);