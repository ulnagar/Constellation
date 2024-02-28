namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;
using System.Collections.Generic;

public class CoordinatorAbsenceDigestEmailViewModel : EmailLayoutBaseViewModel
{
    public Name StudentName { get; set; }
    public string SchoolName { get; set; }
    public static string Link => $"https://acos.aurora.nsw.edu.au/schools/";
    public List<AbsenceEntry> WholeAbsences { get; set; } = new();
    public List<AbsenceEntry> PartialAbsences { get; set; } = new();
}