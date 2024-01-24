namespace Constellation.Application.ThirdPartyConsent.GetTransactionDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record TransactionDetailsResponse(
    ConsentTransactionId Id,
    string StudentId,
    Name Name,
    Grade Grade,
    string School,
    string SubmittedBy,
    DateTime SubmittedAt,
    ConsentMethod SubmissionMethod,
    string SubmissionNotes,
    List<TransactionDetailsResponse.ConsentResponse> Applications)
{
    public sealed record ConsentResponse(
        ConsentId Id,
        ApplicationId ApplicationId,
        string Name,
        bool ConsentProvided,
        bool MostRecentResponse);
}
