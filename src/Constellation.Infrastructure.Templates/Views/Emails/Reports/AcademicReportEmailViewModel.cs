namespace Constellation.Infrastructure.Templates.Views.Emails.Reports;

using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.Templates.Views.Shared;

public sealed class AcademicReportEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/Reports/AcademicReportEmail.cshtml";
    public override string ViewLocation => _viewLocation;
    public readonly string Link = $"{BaseUrl}";

    public required string ParentName { get; set; }
    public required Name StudentName { get; set; }
    public required string ReportingPeriod { get; set; }
    public required string Year { get; set; }
}
