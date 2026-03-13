namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed class StudentAbsenceDigestEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/StudentAbsenceDigestEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public string Link => $"{BaseUrl}";

    public required Name StudentName { get; init; }
    public List<AbsenceEntry> PartialAbsences { get; init; } = [];
    public required StudentId StudentId { get; init; }
}
