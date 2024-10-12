namespace Constellation.Core.Models.ThirdPartyConsent;

using Constellation.Core.Models.Students.Identifiers;
using Enums;
using Identifiers;
using System;
using ApplicationId = Identifiers.ApplicationId;

public sealed class Consent
{
    private Consent() { } // Required by EF Core

    private Consent(
        ConsentTransactionId transactionId,
        ApplicationId applicationId,
        StudentId studentId,
        bool consentProvided,
        string providedBy,
        DateTime providedAt,
        ConsentMethod method,
        string methodNotes)
    {
        Id = new();

        TransactionId = transactionId;
        ApplicationId = applicationId;
        StudentId = studentId;
        ConsentProvided = consentProvided;
        ProvidedBy = providedBy;
        ProvidedAt = providedAt;
        Method = method;
        MethodNotes = methodNotes;
    }

    public ConsentId Id { get; private set; }
    public ApplicationId ApplicationId { get; private set; }
    public ConsentTransactionId TransactionId { get; private set; }
    public StudentId StudentId { get; private set; }
    public bool ConsentProvided { get; private set; }
    public string ProvidedBy { get; private set; }
    public DateTime ProvidedAt { get; private set; }
    public ConsentMethod Method { get; private set; }
    public string MethodNotes {get; private set; }

    public static Consent Create(
        ConsentTransactionId transactionId,
        StudentId studentId,
        ApplicationId applicationId,
        bool consentProvided,
        string submittedBy,
        DateTime submittedAt,
        ConsentMethod submissionMethod,
        string submissionNotes)
    {
        Consent consent = new Consent(
            transactionId,
            applicationId,
            studentId,
            consentProvided,
            submittedBy,
            submittedAt,
            submissionMethod,
            submissionNotes);

        return consent;
    }
}