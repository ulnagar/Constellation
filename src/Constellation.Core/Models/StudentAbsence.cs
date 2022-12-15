namespace Constellation.Core.Models;

using Constellation.Core.Enums;

public class StudentWholeAbsence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StudentId { get; set; } = string.Empty;
    public virtual Student? Student { get; set; }
    public int OfferingId { get; set; }
    public virtual CourseOffering? Offering { get; set; }
    public DateTime Date { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public string PeriodTimeframe { get; set; } = string.Empty;
    public List<WholeAbsenceNotification> Notifications { get; set; } = new();
    public List<WholeAbsenceResponse> Responses { get; set; } = new();
    public bool ExternallyExplained { get; set; }
    public DateTime DateScanned { get; set; }
    public DateTime LastSeen { get; set; }
    public bool Explained => (Responses.Any() || ExternallyExplained);

}

public class WholeAbsenceNotification
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string OutgoingId { get; set; } = string.Empty;
    public virtual StudentWholeAbsence? Absence { get; set; }
    public Guid AbsenceId { get; set; }
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public bool ConfirmedDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string DeliveredMessageIds { get; set; } = string.Empty;
}

public class WholeAbsenceResponse
{
    public WholeAbsenceResponse()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
    public virtual StudentWholeAbsence? Absence { get; set; }
    public Guid AbsenceId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string ReceivedFrom { get; set; } = string.Empty;
    public string ReceivedFromName { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public bool Forwarded { get; set; }
    public DateTime? ForwardedAt { get; set; }
}

public class StudentPartialAbsence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StudentId { get; set; } = string.Empty;
    public virtual Student? Student { get; set; }
    public int OfferingId { get; set; }
    public virtual CourseOffering? Offering { get; set; }
    public DateTime Date { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public string PeriodTimeframe { get; set; } = string.Empty;
    public int PartialAbsenceLength { get; set; }
    public string PartialAbsenceTimeframe { get; set; } = string.Empty;
    public List<PartialAbsenceNotification> Notifications { get; set; } = new();
    public List<PartialAbsenceVerificationNotification> VerificationNotifications { get; set; } = new();
    public List<PartialAbsenceResponse> Responses { get; set; } = new();
    public bool ExternallyExplained { get; set; }
    public DateTime DateScanned { get; set; }
    public DateTime LastSeen { get; set; }
    public bool Explained => (Responses.Any(r => r.Verification == PartialAbsenceVerification.Verified) || ExternallyExplained);
}

public class PartialAbsenceNotification
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string OutgoingId { get; set; } = string.Empty;
    public virtual StudentPartialAbsence? Absence { get; set; }
    public Guid AbsenceId { get; set; }
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = string.Empty; 
    public string Recipients { get; set; } = string.Empty;
}

public class PartialAbsenceVerificationNotification
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string OutgoingId { get; set; } = string.Empty;
    public virtual StudentPartialAbsence? Absence { get; set; }
    public Guid AbsenceId { get; set; }
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
}

public class PartialAbsenceResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public virtual StudentPartialAbsence? Absence { get; set; }
    public Guid AbsenceId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public PartialAbsenceVerification Verification { get; set; }
    public virtual SchoolContact? Verifier { get; set; }
    public DateTime VerifiedAt { get; set; }
    public string VerifiedComment { get; set; } = string.Empty;
}
