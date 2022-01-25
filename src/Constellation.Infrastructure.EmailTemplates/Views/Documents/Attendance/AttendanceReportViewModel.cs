using Constellation.Application.Extensions;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class AttendanceReportViewModel
    {
        public AttendanceReportViewModel()
        {
            Absences = new List<AbsenceDto>();
            DateData = new List<DateSessions>();
            ExcludedDates = new List<DateTime>();
        }

        public string StudentName { get; set; }
        public DateTime StartDate { get; set; }
        public ICollection<DateSessions> DateData { get; set; }
        public ICollection<AbsenceDto> Absences { get; set; }
        public ICollection<DateTime> ExcludedDates { get; set; }

        public class AbsenceDto
        {
            public DateTime Date { get; set; }
            public int OfferingId { get; set; }
            public TimeSpan StartTime { get; set; }
            public string Type { get; set; }
            public string AbsenceTimeframe { get; set; }
            public string AbsenceReason { get; set; }
            public string ExplanationString { get; set; }

            public static AbsenceDto ConvertFromAbsence(Absence absence)
            {
                var viewModel = new AbsenceDto
                {
                    Date = absence.Date,
                    OfferingId = absence.OfferingId,
                    StartTime = absence.StartTime,
                    Type = absence.Type,
                    AbsenceReason = absence.AbsenceReason,
                    AbsenceTimeframe = absence.AbsenceTimeframe,
                };

                if (absence.ExternallyExplained && string.IsNullOrWhiteSpace(absence.ExternalExplanation))
                    viewModel.ExplanationString = $"<br />Explained via Sentral";

                if (absence.ExternallyExplained && !string.IsNullOrWhiteSpace(absence.ExternalExplanation))
                    viewModel.ExplanationString = $"<br />Explained via Sentral: {absence.ExternalExplanation} ({absence.ExternalExplanationSource})";

                if (absence.Responses.Any())
                    foreach (var response in absence.Responses)
                        viewModel.ExplanationString += $"<br />Explained via Portal: {response.Explanation} ({response.From})";

                if (!absence.Explained)
                    viewModel.ExplanationString = "No explanation provided yet.";

                return viewModel;
            }
        }

        public class DateSessions
        {
            public DateSessions()
            {
                SessionsByOffering = new List<SessionByOffering>();
            }

            public DateTime Date { get; set; }
            public int DayNumber { get; set; }
            public ICollection<SessionByOffering> SessionsByOffering { get; set; }
        }

        public class SessionByOffering
        {
            public string PeriodName { get; set; }
            public string PeriodTimeframe { get; set; }
            public string OfferingName { get; set; }
            public string CourseName { get; set; }
            public int OfferingId { get; set; }

            public static SessionByOffering ConvertFromSessionGroup(IGrouping<int, OfferingSession> sessions)
            {
                var viewModel = new SessionByOffering
                {
                    PeriodName = (sessions.Count() > 1) ? $"{sessions.First().Period.Name} - {sessions.Last().Period.Name}" : sessions.First().Period.Name,
                    PeriodTimeframe = $"{sessions.First().Period.StartTime.As12HourTime()} - {sessions.Last().Period.EndTime.As12HourTime()}",
                    OfferingName = sessions.First().Offering.Name,
                    CourseName = sessions.First().Offering.Course.Name,
                    OfferingId = sessions.Key
                };

                return viewModel;
            }

        }
    }
}