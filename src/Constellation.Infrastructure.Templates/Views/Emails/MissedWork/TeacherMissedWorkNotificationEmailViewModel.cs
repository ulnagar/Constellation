namespace Constellation.Infrastructure.Templates.Views.Emails.MissedWork;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;
using System.Collections.Generic;

public class TeacherMissedWorkNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public string OfferingName { get; set; }
    public DateOnly AbsenceDate { get; set; }
    public List<string> StudentList { get; set; } = new();
    public string Link { get; set; }
    public bool IsCovered { get; set; }
}
