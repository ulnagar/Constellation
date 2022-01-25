using Constellation.Core.Models;
using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.Absences
{
    public class StudentAbsenceExplanationRequestEmailViewModel : EmailLayoutBaseViewModel
    {
        public StudentAbsenceExplanationRequestEmailViewModel()
        {
            Absences = new List<AbsenceEntry>();
        }

        public string StudentName { get; set; }
        public string Link { get; set; }
        public ICollection<AbsenceEntry> Absences { get; set; }

        public class AbsenceEntry
        {
            public DateTime Date { get; set; }
            public string PeriodName { get; set; }
            public string PeriodTimeframe { get; set; }
            public string OfferingName { get; set; }
            public string AbsenceTimeframe { get; set; }

            public static AbsenceEntry ConvertFromAbsence(Absence absence)
            {
                var viewModel = new AbsenceEntry
                {
                    Date = absence.Date,
                    PeriodName = absence.PeriodName,
                    PeriodTimeframe = absence.PeriodTimeframe,
                    OfferingName = absence.Offering.Name,
                    AbsenceTimeframe = absence.AbsenceTimeframe
                };

                return viewModel;
            }
        }
    }
}
