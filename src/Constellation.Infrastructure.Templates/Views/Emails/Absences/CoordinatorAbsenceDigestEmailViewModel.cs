namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

public class CoordinatorAbsenceDigestEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string SchoolName { get; set; }
    public static string Link => $"https://acos.aurora.nsw.edu.au/schools/";
    public List<AbsenceEntry> Absences { get; set; } = new();

    public class AbsenceEntry
    {
        public DateTime Date { get; set; }
        public string PeriodName { get; set; }
        public string PeriodTimeframe { get; set; }
        public string OfferingName { get; set; }

        public static AbsenceEntry ConvertFromAbsence(Absence absence, CourseOffering offering)
        {
            var viewModel = new AbsenceEntry
            {
                Date = absence.Date.ToDateTime(TimeOnly.MinValue),
                PeriodName = absence.PeriodName,
                PeriodTimeframe = absence.PeriodTimeframe,
                OfferingName = offering.Name
            };

            return viewModel;
        }
    }
}