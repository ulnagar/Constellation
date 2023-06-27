namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Application.Absences.ConvertResponseToAbsenceExplanation;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public class CoordinatorAbsenceVerificationRequestEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string SchoolName { get; set; }
    public static string PortalLink = $"https://acos.aurora.nsw.edu.au/schools/";
    public List<AbsenceExplanation> ClassList { get; set; } = new();
}
