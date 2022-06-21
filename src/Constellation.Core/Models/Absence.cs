using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Core.Models
{
    public class Absence
    {
        public Absence()
        {
            Notifications = new List<AbsenceNotification>();
            Responses = new List<AbsenceResponse>();

            ClassworkNotifications = new List<ClassworkNotification>();

            Id = Guid.Empty;
        }

        public class Types
        {
            public const string Partial = "Partial";
            public const string Whole = "Whole";
        }
        
        public Guid Id { get; set; }
        public string Type { get; set; }

        public string StudentId { get; set; }
        public Student Student { get; set; }

        public int OfferingId { get; set; }
        public CourseOffering Offering { get; set; }

        public DateTime Date { get; set; }
        public string PeriodName { get; set; }
        public string PeriodTimeframe { get; set; }
        public int AbsenceLength { get; set; }
        public string AbsenceTimeframe { get; set; }
        public string AbsenceReason { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool Explained => Responses.Any(response =>
            (response.Type == AbsenceResponse.Types.Student && response.VerificationStatus == AbsenceResponse.VerificationStatuses.Verified) ||
            response.Type == AbsenceResponse.Types.Parent || 
            response.Type == AbsenceResponse.Types.Coordinator);

        public ICollection<AbsenceNotification> Notifications { get; set; }
        public ICollection<AbsenceResponse> Responses { get; set; }
        public ICollection<ClassworkNotification> ClassworkNotifications { get; set; }

        public DateTime DateScanned { get; set; }
        public DateTime LastSeen { get; set; }
    }
}