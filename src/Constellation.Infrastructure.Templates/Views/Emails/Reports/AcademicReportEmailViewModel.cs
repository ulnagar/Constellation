namespace Constellation.Infrastructure.Templates.Views.Emails.Reports;

using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.Templates.Views.Shared;

public class AcademicReportEmailViewModel 
    : EmailLayoutBaseViewModel
{
    public string ParentName { get; set; }
    public Name StudentName { get; set; }
    public string ReportingPeriod { get; set; }
    public string Year { get; set; }
}
