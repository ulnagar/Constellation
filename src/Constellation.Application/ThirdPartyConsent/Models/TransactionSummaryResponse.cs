namespace Constellation.Application.ThirdPartyConsent.Models;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.ThirdPartyConsent.Enums;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record TransactionSummaryResponse(
    ConsentTransactionId TransactionId,
    StudentId StudentId,
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