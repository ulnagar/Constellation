namespace Constellation.Core.Models.Absences;

using Constellation.Core.Models.Identifiers;
using System;

public class Response
{


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
}