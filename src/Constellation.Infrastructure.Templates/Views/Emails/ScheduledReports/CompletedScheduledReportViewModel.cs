namespace Constellation.Infrastructure.Templates.Views.Emails.ScheduledReports;

using Shared;

public sealed class CompletedScheduledReportViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/ScheduledReports/CompletedScheduledReport.cshtml";

    public string Recipient { get; set; }
}
