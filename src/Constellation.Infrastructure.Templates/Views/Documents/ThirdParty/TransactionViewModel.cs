namespace Constellation.Infrastructure.Templates.Views.Documents.ThirdParty;

using Constellation.Core.Models.ThirdPartyConsent.Identifiers;
using Core.Enums;
using Core.Models.ThirdPartyConsent.Enums;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed class TransactionViewModel
{
    public ConsentTransactionId Id { get; init; }
    public Name Student { get; init; }
    public Grade Grade { get; init; }
    public string ProvidedBy { get; init; } = string.Empty;
    public EmailAddress ProvidedByEmail { get; init; }
    public DateTime ProvidedAt { get; init; }
    public ConsentMethod Method { get; init; } = ConsentMethod.PhoneCall;
    public string MethodNotes { get; init; } = string.Empty;
    public List<ConsentItem> Responses { get; init; } = new();
    
    public class ConsentItem
    {
        public Core.Models.ThirdPartyConsent.Identifiers.ApplicationId ApplicationId { get; init; }
        public string ApplicationName { get; init; }
        public string Purpose { get; init; }
        public List<string> InformationCollected { get; init; }
        public string StoredCountry { get; init; }
        public List<string> SharedWith { get; init; }
        public string Link { get; init; }
        public List<string> RequiredBy { get; init; }
        public bool ConsentProvided { get; init; }
    }
}
