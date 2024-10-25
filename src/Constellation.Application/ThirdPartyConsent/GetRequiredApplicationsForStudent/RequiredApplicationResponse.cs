namespace Constellation.Application.ThirdPartyConsent.GetRequiredApplicationsForStudent;

using Core.Models.Students.Identifiers;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using System;
using System.Collections.Generic;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record RequiredApplicationResponse(
    ApplicationId ApplicationId,
    string ApplicationName,
    string Purpose,
    IReadOnlyList<string> InformationCollected,
    string StoredCountry,
    IReadOnlyList<string> SharedWith,
    string Link,
    ConsentId Id,
    ConsentTransactionId TransactionId,
    StudentId StudentId,
    bool ConsentProvided,
    string ProvidedBy,
    DateTime? ProvidedAt,
    ConsentMethod? Method,
    string MethodNotes,
    Dictionary<ConsentRequirementId, string> Requirements);
