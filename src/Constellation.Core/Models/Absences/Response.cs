namespace Constellation.Core.Models.Absences;

using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using System;

public class Response
{
    private Response(
        AbsenceId absenceId,
        DateTime receivedAt,
        ResponseType type,
        string from,
        string explanation)
    {
        Id = new();
        AbsenceId = absenceId;
        ReceivedAt = receivedAt;
        Type = type;
        From = from;
        Explanation = explanation;

        if (Type == ResponseType.Student)
            VerificationStatus = ResponseVerificationStatus.Pending;
        else
            VerificationStatus = ResponseVerificationStatus.NotRequired;
    }

    public AbsenceResponseId Id { get; private set; }
    public AbsenceId AbsenceId { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public ResponseType Type { get; private set; }
    public string From { get; private set; }
    public string Explanation { get; private set; }
    public ResponseVerificationStatus VerificationStatus { get; private set; }
    public string Verifier { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string VerificationComment { get; private set; }
    public bool Forwarded { get; private set; }

    public static Response Create(
        AbsenceId absenceId,
        DateTime receivedAt,
        ResponseType type,
        string from,
        string explanation)
    {
        Response response = new(
            absenceId,
            receivedAt,
            type,
            from,
            explanation);

        return response;
    }

    public void VerifyResponse(
        string verifier,
        string comment)
    {
        //check to ensure this is a student response that requires verification first
        if (VerificationStatus.Equals(ResponseVerificationStatus.NotRequired))
            return;
        
        VerificationStatus = ResponseVerificationStatus.Verified;
        VerificationComment = comment;
        Verifier = verifier;
        VerifiedAt = DateTime.Now;
    }

    public void RejectResponse(
        string verifier,
        string comment)
    {
        //check to ensure this is a student response that requires verification first
        if (VerificationStatus.Equals(ResponseVerificationStatus.NotRequired))
            return;

        VerificationStatus = ResponseVerificationStatus.Rejected;
        VerificationComment = comment;
        Verifier = verifier;
        VerifiedAt = DateTime.Now;
    }

    public void MarkForwarded()
    {
        Forwarded = true;
    }
}