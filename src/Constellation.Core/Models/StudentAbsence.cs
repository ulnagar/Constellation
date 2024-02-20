using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Students;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Core.Models
{
    public class StudentWholeAbsence
    {
        public StudentWholeAbsence()
        {
            Id = Guid.NewGuid();

            Notifications = new List<WholeAbsenceNotification>();
            Responses = new List<WholeAbsenceResponse>();
        }

        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public Student Student { get; set; }
        public OfferingId OfferingId { get; set; }
        public Offering Offering { get; set; }
        public DateTime Date { get; set; }
        public string PeriodName { get; set; }
        public string PeriodTimeframe { get; set; }
        public ICollection<WholeAbsenceNotification> Notifications { get; set; }
        public ICollection<WholeAbsenceResponse> Responses { get; set; }
        public bool ExternallyExplained { get; set; }
        public DateTime DateScanned { get; set; }
        public DateTime LastSeen { get; set; }
        public bool Explained => (Responses.Any() || ExternallyExplained);

    }

    public class WholeAbsenceNotification
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string OutgoingId { get; set; }
        public StudentWholeAbsence Absence { get; set; }
        public Guid AbsenceId { get; set; }
        public DateTime SentAt { get; set; }
        public string Message { get; set; }
        public string Recipients { get; set; }
        public bool ConfirmedDelivered { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string DeliveredMessageIds { get; set; }
    }

    public class WholeAbsenceResponse
    {
        public WholeAbsenceResponse()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public StudentWholeAbsence Absence { get; set; }
        public Guid AbsenceId { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string ReceivedFrom { get; set; }
        public string ReceivedFromName { get; set; }
        public string Explanation { get; set; }
        public bool Forwarded { get; set; }
        public DateTime? ForwardedAt { get; set; }
    }

    public class StudentPartialAbsence
    {
        public StudentPartialAbsence()
        {
            Id = Guid.NewGuid();

            Notifications = new List<PartialAbsenceNotification>();
            VerificationNotifications = new List<PartialAbsenceVerificationNotification>();
            Responses = new List<PartialAbsenceResponse>();
        }

        public Guid Id { get; set; }
        public string StudentId { get; set; }
        public Student Student { get; set; }
        public OfferingId OfferingId { get; set; }
        public Offering Offering { get; set; }
        public DateTime Date { get; set; }
        public string PeriodName { get; set; }
        public string PeriodTimeframe { get; set; }
        public int PartialAbsenceLength { get; set; }
        public string PartialAbsenceTimeframe { get; set; }
        public ICollection<PartialAbsenceNotification> Notifications { get; set; }
        public ICollection<PartialAbsenceVerificationNotification> VerificationNotifications { get; set; }
        public ICollection<PartialAbsenceResponse> Responses { get; set; }
        public bool ExternallyExplained { get; set; }
        public DateTime DateScanned { get; set; }
        public DateTime LastSeen { get; set; }
        public bool Explained => (Responses.Any(r => r.Verification == PartialAbsenceVerification.Verified) || ExternallyExplained);
    }

    public class PartialAbsenceNotification
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string OutgoingId { get; set; }
        public StudentPartialAbsence Absence { get; set; }
        public Guid AbsenceId { get; set; }
        public DateTime SentAt { get; set; }
        public string Message { get; set; }
        public string Recipients { get; set; }
    }

    public class PartialAbsenceVerificationNotification
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string OutgoingId { get; set; }
        public StudentPartialAbsence Absence { get; set; }
        public Guid AbsenceId { get; set; }
        public DateTime SentAt { get; set; }
        public string Message { get; set; }
        public string Recipients { get; set; }
    }

    public class PartialAbsenceResponse
    {
        public PartialAbsenceResponse()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public StudentPartialAbsence Absence { get; set; }
        public Guid AbsenceId { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Explanation { get; set; }
        public PartialAbsenceVerification Verification { get; set; }
        public SchoolContact Verifier { get; set; }
        public DateTime VerifiedAt { get; set; }
        public string VerifiedComment { get; set; }
    }
}
