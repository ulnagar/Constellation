namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Application.Domains.Tutorials.GroupTutorials.Queries.GetCurrentStudentsInGroupTutorial;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public sealed class ParentAbsenceDigestEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/ParentAbsenceDigestEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public static string Link => $"{BaseUrl}";

    public required string ParentName { get; set; }
    public required string StudentFirstName { get; set; }
    public List<AbsenceEntry> WholeAbsences { get; set; } = [];
    public List<AbsenceEntry> PartialAbsences { get; set; } = [];
}
