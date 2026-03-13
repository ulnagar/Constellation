namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public sealed class StudentAbsenceExplanationRequestEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/StudentAbsenceExplanationRequestEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public const string Link = $"{BaseUrl}";

    public required string StudentName { get; set; }
    public List<AbsenceEntry> Absences { get; set; } = [];
}
