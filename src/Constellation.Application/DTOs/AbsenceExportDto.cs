using Constellation.Core.Models;
using System;

namespace Constellation.Application.DTOs
{
    public class AbsenceExportDto
    {
        public string Student { get; set; }
        public string Grade { get; set; }
        public string School { get; set; }
        public bool Explained { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string Period { get; set; }
        public string Length { get; set; }
        public string Timeframe { get; set; }
        public string Class { get; set; }
        public string NotificationCount { get; set; }
        public string ResponseCount { get; set; }

        public static AbsenceExportDto ConvertFromAbsence(Absence absence)
        {
            var viewModel = new AbsenceExportDto
            {
                Student = absence.Student.DisplayName,
                Grade = $"Year {(int)absence.Student.CurrentGrade:D2}",
                School = absence.Student.School.Name,
                Explained = absence.Explained,
                Type = absence.Type,
                Date = absence.Date,
                Period = absence.PeriodName,
                Length = absence.AbsenceLength.ToString(),
                Timeframe = absence.AbsenceTimeframe,
                Class = absence.Offering.Name,
                NotificationCount = absence.Notifications.Count.ToString(),
                ResponseCount = absence.Responses.Count.ToString()
            };

            return viewModel;
        }
    }
}
