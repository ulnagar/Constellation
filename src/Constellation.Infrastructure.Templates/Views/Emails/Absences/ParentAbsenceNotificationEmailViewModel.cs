﻿namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Constellation.Infrastructure.Templates.Views.Shared;
using System.Collections.Generic;

public class ParentAbsenceNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public string ParentName { get; set; }
    public string StudentFirstName { get; set; }
    public static string Link => "https://acos.aurora.nsw.edu.au/Parents";
    public List<AbsenceEntry> Absences { get; set; } = new();
}
