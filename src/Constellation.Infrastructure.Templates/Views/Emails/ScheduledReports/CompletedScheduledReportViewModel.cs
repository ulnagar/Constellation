namespace Constellation.Infrastructure.Templates.Views.Emails.ScheduledReports;

using Shared;

public sealed class CompletedScheduledReportViewModel : EmailLayoutBaseViewModel
{

    private const string _viewLocation = "/Views/Emails/ScheduledReports/CompletedScheduledReport.cshtml";
    public override string ViewLocation => _viewLocation;
    
    public required string Recipient { get; set; }
}
