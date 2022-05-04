using System;

namespace Constellation.Core.Models
{
    public class AbsenceResponse
    {
        // Accepted values for Type
        public const string Coordinator = "Coordinator";
        public const string Parent = "Parent";
        public const string Student = "Student";
        public const string System = "System";

        // Accepted values for VerificationStatus
        public const string NotRequired = "Not Required";
        public const string Pending = "Pending";
        public const string Rejected = "Rejected";
        public const string Verified = "Verified";

        public AbsenceResponse()
        {
            Id = Guid.Empty;
        }

        public Guid Id { get; set; }
        public Guid AbsenceId { get; set; }
        public Absence Absence { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Type { get; set; }
        public string From { get; set; }
        public string Explanation { get; set; }
        public string VerificationStatus { get; set; }
        public string Verifier { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string VerificationComment { get; set; }
        public bool Forwarded { get; set; }
    }
}