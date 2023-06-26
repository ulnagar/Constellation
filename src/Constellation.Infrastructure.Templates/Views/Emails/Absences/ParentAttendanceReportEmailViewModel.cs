namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class ParentAttendanceReportEmailViewModel : EmailLayoutBaseViewModel
{
    public string StudentName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
