namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.UpdateTempReportDetails;

using Constellation.Core.Models.Reports.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Core.Models.Reports.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Shared.Helpers.ModelBinders;

public sealed class UpdateTempReportDetailsViewModel
{
    public ExternalReportId ReportId { get; set; }
    public string FileName { get; set; }
    public StudentId StudentId { get; set; }
    [ModelBinder(typeof(BaseFromValueBinder))]
    public ReportType ReportType { get; set; }
    public DateOnly IssuedDate { get; set; }

    public SelectList Students { get; set; }
}
