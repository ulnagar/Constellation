namespace Constellation.Application.DTOs;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public class AbsenceResponseEmail
{
    public List<EmailRecipient> Recipients { get; set; } = [];
    public List<AbsenceDto> WholeAbsences { get; private set; } = [];
    public string StudentName { get; set; } = string.Empty;

    public class AbsenceDto
    {
        public string ReportedBy { get; set; }
        public DateTime AbsenceDate { get; set; }
        public string PeriodName { get; set; }
        public string ClassName { get; set; }
        public string Explanation { get; set; }
        public AbsenceType AbsenceType { get; set; }
        public string AbsenceTimeframe { get; set; }

        public AbsenceDto(Absence absence, Response response, string activityName)
        {
            ReportedBy = "UNKNOWN SOURCE";

            if (response.Type == ResponseType.Coordinator)
                ReportedBy = $"Reported by {response.From} (ACC)";
            else if (response.Type == ResponseType.Parent)
                ReportedBy = "Reported by Parent";
            else if (response.Type == ResponseType.Student)
            {
                var status = (response.VerificationStatus == ResponseVerificationStatus.Verified)
                    ? "verified"
                    : "rejected";

                ReportedBy = $"Reported by Student and <strong>{status}</strong> by {response.Verifier} (ACC)";

                if (!string.IsNullOrWhiteSpace(response.VerificationComment))
                    ReportedBy += $"<br />with comment: {response.VerificationComment}";
            }

            AbsenceDate = absence.Date.ToDateTime(TimeOnly.MinValue);
            PeriodName = $"{absence.PeriodName} ({absence.PeriodTimeframe})";
            ClassName = activityName;
            Explanation = response.Explanation;
            AbsenceType = absence.Type;
            AbsenceTimeframe = absence.AbsenceTimeframe;
        }

        public AbsenceDto(Absence absence, string activityName, string email, string explanation)
        {
            ReportedBy = $"Reported by Parent ({email})";

            AbsenceDate = absence.Date.ToDateTime(TimeOnly.MinValue);
            PeriodName = $"{absence.PeriodName} ({absence.PeriodTimeframe})";
            ClassName = activityName;
            Explanation = explanation;
            AbsenceType = absence.Type;
            AbsenceTimeframe = absence.AbsenceTimeframe;
        }
    }
}