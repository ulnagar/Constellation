﻿namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public class StudentAbsenceExplanationRequestEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string Link { get; set; }
    public List<AbsenceEntry> Absences { get; set; } = new();
}
