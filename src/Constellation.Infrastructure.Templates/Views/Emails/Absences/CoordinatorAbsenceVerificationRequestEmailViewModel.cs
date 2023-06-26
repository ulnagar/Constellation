namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

public class CoordinatorAbsenceVerificationRequestEmailViewModel : EmailLayoutBaseViewModel
{
    public const string PortalLink = $"https://acos.aurora.nsw.edu.au/schools/";

    public string StudentName { get; set; }
    public string SchoolName { get; set; }
    public List<AbsenceExplanation> ClassList { get; set; } = new();

    public class AbsenceExplanation
    {
        public DateOnly Date { get; set; }
        public string PeriodName { get; set; }
        public string PeriodTimeframe { get; set; }
        public string OfferingName { get; set; }
        public string AbsenceTimeframe { get; set; }
        public string Explanation { get; set; }
    }
}
