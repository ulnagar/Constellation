namespace Constellation.Application.ThirdPartyConsent.GetConsentDetails;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.ValueObjects;
using System;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record ConsentDetailsResponse(
    ConsentId Id,
    ConsentTransactionId TransactionId,
    ApplicationId ApplicationId,
    string ApplicationName,
    string ApplicationPurpose,
    string[] ApplicationInformationCollected,
    string ApplicationStoredCountry,
    string[] ApplicationSharedWith,
    bool ApplicationConsentRequired,
    StudentId StudentId,
    Name StudentName,
    Grade StudentGrade,
    string SchoolName,
    bool ConsentProvided,
    string ProvidedBy,
    DateTime ProvidedAt,
    ConsentMethod Method,
    string MethodNotes);
