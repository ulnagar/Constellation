namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Shared;
using System;
using ApplicationId = Identifiers.ApplicationId;

public static class ConsentErrors
{
    public static class Application
    {
        public static Func<ApplicationId, Error> NotFound = id => new(
            "Consent.Application.NotFound",
            $"Could not find an Application with Id {id.Value}");
    }

    public static class ConsentMethod
    {
        public static Error ValueEmpty = new(
            "Consent.ConsentMethod.ValueEmpty",
            "A valid value must be provided for the Consent Method");
    }

    public static class Transaction
    {
        public static Error NoResponses = new(
            "Consent.Transaction.NoResponses",
            "At least one response must be included when submitting a Consent Transaction");
    }
}