using System;

namespace Constellation.Application.Models
{
    public class EmailToProcess
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; }
        public EmailStatus Status { get; set; }
        public DateTime Created { get; set; }
        public int FailureCount { get; set; }
        public string FailureMessage { get; set; }
        public DateTime? SentAt { get; set; }

        public enum EmailStatus
        {
            Ready,
            Failed,
            Sent,
            Cancelled
        }
    }
}