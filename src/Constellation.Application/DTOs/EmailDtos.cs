namespace Constellation.Application.DTOs;

using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using System;
using System.Collections.Generic;
using System.Net.Mail;

public partial class EmailDtos
{

    public class AbsenceResponseEmail
    {
        public List<string> Recipients { get; set; }
        public List<AbsenceDto> WholeAbsences { get; private set; } = new();
        public ICollection<Absence> PartialAbsences { get; set; }
        public string StudentName { get; set; }

        public AbsenceResponseEmail()
        {
            Recipients = new List<string>();
            PartialAbsences = new List<Absence>();
        }

        public class AbsenceDto
        {
            public string ReportedBy { get; set; }
            public DateTime AbsenceDate { get; set; }
            public string PeriodName { get; set; }
            public string ClassName { get; set; }
            public string Explanation { get; set; }
            public AbsenceType AbsenceType { get; set; }
            public string AbsenceTimeframe { get; set; }

            public AbsenceDto(Absence absence, Response response, CourseOffering offering)
            {
                ReportedBy = "UNKNOWN SOURCE";

                if (response.Type == ResponseType.Coordinator)
                    ReportedBy = $"Reported by {response.From} (ACC)";
                else if (response.Type == ResponseType.Parent)
                    ReportedBy = "Reported by Parent";
                else if (response.Type == ResponseType.Student)
                {
                    var status = (response.VerificationStatus == ResponseVerificationStatus.Verified) ? "verified" : "rejected";

                    ReportedBy = $"Reported by Student and <strong>{status}</strong> by {response.Verifier} (ACC)";

                    if (!string.IsNullOrWhiteSpace(response.VerificationComment))
                        ReportedBy += $"<br />with comment: {response.VerificationComment}";
                }

                AbsenceDate = absence.Date.ToDateTime(TimeOnly.MinValue);
                PeriodName = $"{absence.PeriodName} ({absence.PeriodTimeframe})";
                ClassName = offering.Name;
                Explanation = response.Explanation;
                AbsenceType = absence.Type;
                AbsenceTimeframe = absence.AbsenceTimeframe;
            }
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
