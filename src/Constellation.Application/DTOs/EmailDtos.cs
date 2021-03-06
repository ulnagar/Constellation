using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Constellation.Application.DTOs
{
    public class EmailDtos
    {
        public class LessonEmail
        {
            public LessonEmail()
            {
                Lessons = new List<LessonItem>();
            }

            public string SchoolCode { get; set; }
            public string SchoolName { get; set; }
            public ICollection<LessonItem> Lessons { get; set; }

            public class LessonItem
            {
                public string Name { get; set; }
                public DateTime DueDate { get; set; }
                public int OverdueSeverity { get; set; }
            }
        }

        public class AbsenceResponseEmail
        {
            public List<string> Recipients { get; set; }
            public ICollection<Absence> WholeAbsences { get; set; }
            public ICollection<Absence> PartialAbsences { get; set; }

            public AbsenceResponseEmail()
            {
                Recipients = new List<string>();
                WholeAbsences = new List<Absence>();
                PartialAbsences = new List<Absence>();
            }
        }

        public class SentEmail
        {
            public string id { get; set; }
            public string message { get; set; }
            public string recipients { get; set; }
        }

        public class CoverEmail
        {
            public CoverEmail()
            {
                ClassroomTeachers = new Dictionary<string, string>();
                SecondaryRecipients = new Dictionary<string, string>();
                ClassesIncluded = new Dictionary<string, string>();
                Attachments = new List<Attachment>();
            }

            public string CoveringTeacherName { get; set; }
            public string CoveringTeacherEmail { get; set; }
            public bool CoveringTeacherAdobeAccount { get; set; }

            public IDictionary<string, string> ClassroomTeachers { get; set; }
            public IDictionary<string, string> SecondaryRecipients { get; set; }

            public IDictionary<string, string> ClassesIncluded { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public ICollection<Attachment> Attachments { get; set; }
        }

        public class EmailContent
        {
            public ICollection<string> ToAddresses { get; set; }
            public ICollection<string> CcAddresses { get; set; }
            public ICollection<string> BccAddresses { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }

            public string FromAddress { get; set; } = "support@aurora.nsw.edu.au";
            public string FromName { get; set; } = "Aurora College";

            public string SignatureName { get; set; }
            public string SignatureTitle { get; set; }


            public EmailContent()
            {
                ToAddresses = new List<string>();
                CcAddresses = new List<string>();
                BccAddresses = new List<string>();
            }
        }
    }
}
