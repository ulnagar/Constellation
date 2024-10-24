namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Identifiers;
using Shared;
using System;
using ApplicationId = Identifiers.ApplicationId;

public static class ConsentRequirementErrors
{
    public static readonly Func<Type, string, Error> AlreadyExists = (type, id) => new(
        "Consent.ConsentRequirement.AlreadyExists",
        $"A Consent Requirement of type {type} for reference {id} already exists");

    public static readonly Func<ApplicationId, ConsentRequirementId, Error> NotFound = (appId, reqId) => new(
        "Consent.ConsentRequirement.NotFound",
        $"Could not find a Consent Requirement with id {reqId} in Application {appId}");
}