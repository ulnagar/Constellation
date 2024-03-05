namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Identifiers;
using Shared;
using System;
using ApplicationId = Identifiers.ApplicationId;

public static class ConsentErrors
{
    public static class Application
    {
        public static readonly Func<ApplicationId, Error> NotFound = id => new(
            "Consent.Application.NotFound",
            $"Could not find an Application with Id {id.Value}");

        public static readonly Func<ApplicationId, string, Error> NotRequired = (id, name) => new(
            "Consent.Application.NotRequired",
            $"Consent is not required for {name} ({id})");
    }

    public static class Consent
    {
        public static readonly Func<ConsentId, Error> NotFound = id => new(
            "Consent.Consent.NotFound",
            $"Could not find a Consent Response with Id {id.Value}");
    }

    public static class ConsentMethod
    {
        public static Error ValueEmpty = new(
            "Consent.ConsentMethod.ValueEmpty",
            "A valid value must be provided for the Consent Method");
    }

    public static class Transaction
    {
        public static readonly Error NoResponses = new(
            "Consent.Transaction.NoResponses",
            "At least one response must be included when submitting a Consent Transaction");

        public static readonly Func<ConsentTransactionId, Error> NotFound = id => new(
            "Consent.Transaction.NotFound",
            $"Could not find a Transaction with Id {id.Value}");
    }
}