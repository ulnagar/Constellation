namespace Constellation.Infrastructure.Templates.Views.Emails.MissedWork;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class StudentMissedWorkNotificationEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public string CourseName { get; set; }
    public string OfferingName { get; set; }
    public DateOnly AbsenceDate { get; set; }
    public string WorkDescription { get; set; }
}
