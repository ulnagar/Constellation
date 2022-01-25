using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Constellation.Application.DTOs.EmailRequests
{
    public class AttendanceReportEmail : EmailBaseClass
    {
        public AttendanceReportEmail()
        {
            Attachments = new List<Attachment>();
        }

        public ICollection<Attachment> Attachments { get; set; }
        public string StudentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public NotificationSequence NotificationType { get; set; }

        public enum NotificationSequence
        {
            Student,
            School
        }
    }
}
