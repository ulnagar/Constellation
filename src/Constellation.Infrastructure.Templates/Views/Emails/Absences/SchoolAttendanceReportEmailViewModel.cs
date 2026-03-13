namespace Constellation.Infrastructure.Templates.Views.Emails.Absences;

using Constellation.Infrastructure.Templates.Views.Shared;
using System;

public sealed class SchoolAttendanceReportEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Absences/SchoolAttendanceReportEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }
}
