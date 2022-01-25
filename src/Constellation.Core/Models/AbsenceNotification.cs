using System;

namespace Constellation.Core.Models
{
    public class AbsenceNotification
    {
        public const string SMS = "SMS";
        public const string Email = "Email";

        public AbsenceNotification()
        {
            Id = Guid.Empty;
        }

        public Guid Id { get; set; }

        public Guid AbsenceId { get; set; }
        public Absence Absence { get; set; }

        public string Type { get; set; }
        public string OutgoingId { get; set; }
        public DateTime SentAt { get; set; }
        public string Message { get; set; }
        public string Recipients { get; set; }
        public bool ConfirmedDelivered { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string DeliveredMessageIds { get; set; }
    }
}