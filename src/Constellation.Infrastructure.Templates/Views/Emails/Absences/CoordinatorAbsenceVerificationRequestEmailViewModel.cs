using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

namespace Constellation.Infrastructure.Templates.Views.Emails.Absences
{
    public class CoordinatorAbsenceVerificationRequestEmailViewModel : EmailLayoutBaseViewModel
    {
        public CoordinatorAbsenceVerificationRequestEmailViewModel()
        {
            ClassList = new List<AbsenceExplanation>();
        }

        public string StudentName { get; set; }
        public string SchoolName { get; set; }
        public string PortalLink => $"https://web1.auroracoll-h.schools.nsw.edu.au/Portal/School/";
        //public string PortalLink => $"https://acos.aurora.nsw.edu.au/";
        public ICollection<AbsenceExplanation> ClassList { get; set; }

        public class AbsenceExplanation
        {
            public DateTime Date { get; set; }
            public string PeriodName { get; set; }
            public string PeriodTimeframe { get; set; }
            public string OfferingName { get; set; }
            public string AbsenceTimeframe { get; set; }
            public string Explanation { get; set; }
        }
    }
}
