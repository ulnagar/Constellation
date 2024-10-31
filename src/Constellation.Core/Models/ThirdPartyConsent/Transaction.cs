#nullable enable
namespace Constellation.Core.Models.ThirdPartyConsent;

using Constellation.Core.Enums;
using Enums;
using Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class Transaction
{
    private Transaction() { }

    public ConsentTransactionId Id { get; init; }
    public Name Student { get; init; }
    public Grade Grade { get; init; }
    public string ProvidedBy { get; init; } = string.Empty;
    public DateTime ProvidedAt { get; init; }
    public ConsentMethod Method { get; init; } = ConsentMethod.PhoneCall;
    public string MethodNotes { get; init; } = string.Empty;
    public List<ConsentResponse> Responses { get; init; } = new();

    public sealed class ConsentResponse
    {
        private ConsentResponse() { }

        public ConsentResponse(
            Identifiers.ApplicationId id,
            string name,
            string purpose,
            string[] informationCollected,
            string storedCountry,
            string[] sharedWith,
            string link,
            List<string> requiredBy,
            bool consentProvided)
        {
            ApplicationId = id;
            ApplicationName = name;
            Purpose = purpose;
            InformationCollected = informationCollected.ToList();
            StoredCountry = storedCountry;
            SharedWith = sharedWith.ToList();
            Link = link;
            RequiredBy = requiredBy;
            ConsentProvided = consentProvided;
        }

        public Identifiers.ApplicationId ApplicationId { get; init; }
        public string ApplicationName { get; init; }
        public string Purpose { get; init; }
        public List<string> InformationCollected { get; init; }
        public string StoredCountry { get; init; }
        public List<string> SharedWith { get; init; }
        public string Link { get; init; }
        public List<string> RequiredBy { get; init; }
        public bool ConsentProvided { get; init; }
    }

    public static Transaction Create(
        ConsentTransactionId id,
        Name student,
        Grade grade,
        string providedBy,
        DateTime providedAt,
        ConsentMethod method,
        string notes,
        List<ConsentResponse> responses)
    {
        Transaction transaction = new()
        {
            Id = id,
            Student = student,
            Grade = grade,
            ProvidedBy = providedBy,
            ProvidedAt = providedAt,
            Method = method,
            MethodNotes = notes,
            Responses = responses
        };

        return transaction;
    }
}