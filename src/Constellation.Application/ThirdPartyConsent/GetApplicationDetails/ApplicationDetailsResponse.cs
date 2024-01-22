namespace Constellation.Application.ThirdPartyConsent.GetApplicationDetails;

using Core.Enums;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record ApplicationDetailsResponse(
    ApplicationId Id,
    string Name,
    string Purpose,
    string[] InformationCollected,
    string StoredCountry,
    string[] SharedWith,
    bool ConsentRequired,
    bool IsDeleted,
    List<ApplicationDetailsResponse.ConsentResponse> Consents)
{
    public sealed record ConsentResponse(
        ConsentId Id,
        ConsentTransactionId TransactionId,
        string StudentId,
        Name Name,
        Grade Grade,
        string SchoolName,
        bool ConsentProvided,
        string ProvidedBy,
        DateTime ProvidedAt,
        ConsentMethod Method,
        string MethodNotes);
}