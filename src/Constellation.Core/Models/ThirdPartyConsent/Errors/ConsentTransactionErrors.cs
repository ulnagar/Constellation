namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Constellation.Core.Shared;
using System;

public static class ConsentTransactionErrors
{
    public static readonly Func<ConsentTransactionId, Error> NotFound = id => new(
        "Consent.Transaction.NotFound",
        $"Could not find a Consent Transaction with Id {id.Value}");
}