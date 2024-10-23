namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Constellation.Core.Shared;

public static class ConsentMethodErrors
{
    public static Error ValueEmpty = new(
        "Consent.ConsentMethod.ValueEmpty",
        "A valid value must be provided for the Consent Method");
}