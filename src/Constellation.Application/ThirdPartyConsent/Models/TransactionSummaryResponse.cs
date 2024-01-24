using System;

namespace Constellation.Application.ThirdPartyConsent.Models;

using Core.Enums;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record TransactionSummaryResponse(
    ConsentTransactionId TransactionId,
    string StudentId,
    Name Name,
    Grade Grade,
    string School,
    string SubmittedBy,
    DateTime SubmittedAt,
    ConsentMethod SubmissionMethod,
    string SubmissionNotes,
    List<TransactionSummaryResponse.ConsentStatusResponse> Applications)
{
    public sealed record ConsentStatusResponse(
        string ApplicationName,
        bool ConsentProvided,
        bool MostRecentResponse);
}