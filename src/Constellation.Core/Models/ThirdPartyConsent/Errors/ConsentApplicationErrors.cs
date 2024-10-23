namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Constellation.Core.Shared;

public static class ConsentApplicationErrors
{
    public static readonly System.Func<ApplicationId, Error> NotFound = id => new(
        "Consent.Application.NotFound",
        $"Could not find an Application with Id {id.Value}");

    public static readonly System.Func<ApplicationId, string, Error> NotRequired = (id, name) => new(
        "Consent.Application.NotRequired",
        $"Consent is not required for {name} ({id})");
}