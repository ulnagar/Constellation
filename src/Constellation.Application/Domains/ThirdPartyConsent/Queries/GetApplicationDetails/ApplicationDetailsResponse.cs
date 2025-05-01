namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetApplicationDetails;

using Core.Enums;
using Core.Models.Students.Identifiers;
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
    string Link,
    bool ConsentRequired,
    bool IsDeleted,
    List<ApplicationDetailsResponse.ConsentResponse> Consents,
    List<ApplicationDetailsResponse.Requirement> Requirements)
{
    public sealed record ConsentResponse(
        ConsentId Id,
        ConsentTransactionId TransactionId,
        StudentId StudentId,
        Name Name,
        Grade Grade,
        string SchoolName,
        bool ConsentProvided,
        string ProvidedBy,
        DateTime ProvidedAt,
        ConsentMethod Method,
        string MethodNotes);

    public sealed record Requirement(
        ConsentRequirementId Id,
        string Type,
        string Description,
        DateOnly EnteredOn);
}