namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public class SchoolAttendanceReportEmailViewModel : EmailLayoutBaseViewModel
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
