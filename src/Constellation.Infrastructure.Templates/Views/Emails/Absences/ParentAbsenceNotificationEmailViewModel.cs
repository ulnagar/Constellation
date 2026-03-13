namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public sealed class ParentAbsenceNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/ParentAbsenceNotificationEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public static string Link => $"{BaseUrl}";

    public required string ParentName { get; set; }
    public required string StudentFirstName { get; set; }
    public List<AbsenceEntry> Absences { get; set; } = [];
}
