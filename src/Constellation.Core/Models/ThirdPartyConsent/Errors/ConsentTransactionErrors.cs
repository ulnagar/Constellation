namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Constellation.Core.Shared;
using System;

public static class ConsentTransactionErrors
{
    public static readonly Error NoResponses = new(
        "Consent.Transaction.NoResponses",
        "At least one response must be included when submitting a Consent Transaction");

    public static readonly Func<ConsentTransactionId, Error> NotFound = id => new(
        "Consent.Transaction.NotFound",
        $"Could not find a Transaction with Id {id.Value}");
}