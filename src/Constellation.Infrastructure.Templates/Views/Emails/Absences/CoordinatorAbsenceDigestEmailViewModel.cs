namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public class CoordinatorAbsenceDigestEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string SchoolName { get; set; }
    public static string Link => $"https://acos.aurora.nsw.edu.au/schools/";
    public List<AbsenceEntry> Absences { get; set; } = new();
}