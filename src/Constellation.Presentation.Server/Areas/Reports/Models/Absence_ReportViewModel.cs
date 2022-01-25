using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Absence_ReportViewModel : BaseViewModel
    {
        public Absence_ReportViewModel()
        {
            Absences = new List<AbsenceDto>();
        }

        public ICollection<AbsenceDto> Absences { get; set; }
        public AbsenceFilterDto Filter { get; set; }

        public class AbsenceDto
        {
            public Guid Id { get; set; }
            public string StudentName { get; set; }
            public Grade StudentGrade { get; set; }
            public string SchoolName { get; set; }
            public string SortName { get; set; }
            public bool IsExplained { get; set; }
            public string Type { get; set; }
            public DateTime Date { get; set; }
            public string AbsenceTimeframe { get; set; }
            public string PeriodName { get; set; }
            public string OfferingName { get; set; }
            public int NotificationCount { get; set; }
            public int ResponseCount { get; set; }

            public static AbsenceDto ConvertFromAbsence(Absence absence)
            {
                var viewModel = new AbsenceDto
                {
                    Id = absence.Id,
                    StudentName = absence.Student.DisplayName,
                    StudentGrade = absence.Student.CurrentGrade,
                    SchoolName = absence.Student.School.Name,
                    SortName = $"{absence.Student.LastName} {absence.Student.FirstName}",
                    IsExplained = absence.Explained || absence.ExternallyExplained,
                    Type = absence.Type,
                    Date = absence.Date,
                    AbsenceTimeframe = absence.AbsenceTimeframe,
                    PeriodName = absence.PeriodName,
                    OfferingName = absence.Offering.Name,
                    NotificationCount = absence.Notifications.Count,
                    ResponseCount = absence.Responses.Count
                };

                return viewModel;
            }
        }
    }
}