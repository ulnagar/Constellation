namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Identifiers;
using Shared;
using System;

public static class ConsentErrors
{
    public static readonly Func<ConsentId, Error> NotFound = id => new(
        "Consent.Consent.NotFound",
        $"Could not find a Consent Response with Id {id.Value}");
}