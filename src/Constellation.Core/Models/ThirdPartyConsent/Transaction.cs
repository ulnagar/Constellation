#nullable enable
namespace Constellation.Core.Models.ThirdPartyConsent;

using Constellation.Core.Models.Students.Identifiers;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationId = Identifiers.ApplicationId;

public sealed class Transaction : AggregateRoot
{
    private readonly List<Consent> _consents = new();

    private Transaction() { } // Required by EF Core

    private Transaction(
        StudentId studentId,
        string submittedBy,
        DateTime submittedAt,
        ConsentMethod submissionMethod,
        string submissionNotes)
    {
        Id = new();
        StudentId = studentId;
        SubmittedBy = submittedBy;
        SubmittedAt = submittedAt;
        SubmissionMethod = submissionMethod;
        SubmissionNotes = submissionNotes;
    }

    public ConsentTransactionId Id { get; private init; }
    public StudentId StudentId { get; private init; }
    public string SubmittedBy { get; private init; }
    public DateTime SubmittedAt { get; private init; }
    public ConsentMethod SubmissionMethod { get; private init; }
    public string SubmissionNotes { get; private init; }

    public IReadOnlyList<Consent> Consents => _consents.AsReadOnly();

    public static Result<Transaction> Create(
        StudentId studentId,
        string submittedBy,
        DateTime submittedAt,
        ConsentMethod submissionMethod,
        string submissionNotes,
        Dictionary<ApplicationId, bool> consentResponses)
    {
        if (!consentResponses.Any())
        {
            return Result.Failure<Transaction>(ConsentErrors.Transaction.NoResponses);
        }

        Transaction transaction = new(
            studentId,
            submittedBy,
            submittedAt,
            submissionMethod,
            submissionNotes);

        foreach (KeyValuePair<ApplicationId, bool> response in consentResponses)
        {
            transaction._consents.Add(Consent.Create(
                transaction.Id,
                transaction.StudentId,
                response.Key,
                response.Value,
                transaction.SubmittedBy,
                transaction.SubmittedAt,
                transaction.SubmissionMethod,
                transaction.SubmissionNotes));
        }

        transaction.RaiseDomainEvent(new ConsentTransactionReceivedDomainEvent(new(), transaction.Id));

        return transaction;
    }
}