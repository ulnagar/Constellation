namespace Constellation.Infrastructure.Templates.Views.Documents.ThirdParty;

using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Core.Enums;
using Core.Models.ThirdPartyConsent.Enums;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed class TransactionViewModel
{
    public ConsentTransactionId Id { get; set; }
    public string SubmittedBy { get; set; }
    public DateTime SubmittedAt { get; set; }
    public ConsentMethod SubmissionMethod { get; set; }
    public string SubmissionNotes { get; set; }

    public string StudentId { get; set; }
    public Name Name { get; set; }
    public string School { get; set; }
    public Grade Grade { get; set; }

    public List<ConsentItem> Consents { get; set; } = new();

    public class ConsentItem
    {
        public Core.Models.ThirdPartyConsent.Identifiers.ApplicationId Id { get; set; }
        public string Name { get; set; }
        public string Purpose { get; set; }
        public string[] InformationCollected { get; set; }
        public string StoredCountry { get; set; }
        public string[] SharedWith { get; set; }
        public bool IsDeleted { get; set; }

        public bool ConsentProvided { get; set; }
    }
}
