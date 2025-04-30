namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed class StudentAbsenceDigestEmailViewModel : EmailLayoutBaseViewModel
{
    public Name StudentName { get; init; }
    public List<AbsenceEntry> PartialAbsences { get; init; } = new();
    public StudentId StudentId { get; init; }   

    public string Link => $"https://acos.aurora.nsw.edu.au/Portal/Absences/Students/{StudentId}";
}
