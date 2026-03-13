namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed class CoordinatorAbsenceDigestEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/CoordinatorAbsenceDigestEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public const string Link = $"{BaseUrl}";
    
    public required Name StudentName { get; set; }
    public required string SchoolName { get; set; }
    public List<AbsenceEntry> WholeAbsences { get; set; } = [];
    public List<AbsenceEntry> PartialAbsences { get; set; } = [];
}