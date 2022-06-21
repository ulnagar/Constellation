using System;
using System.Collections.Generic;

namespace Constellation.Core.Models
{
    public class AbsenceNotification
    {
        public class Types
        {
            public const string SMS = "SMS";
            public const string Email = "Email";
        }

        public Guid Id { get; set; }

        public Guid AbsenceId { get; set; }
        public Absence Absence { get; set; }

        public string Type { get; set; }
        public string OutgoingId { get; set; }
        public DateTime SentAt { get; set; }
        public string Message { get; set; }
        public ICollection<string> Recipients { get; set; }
    }
}