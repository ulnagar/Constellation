namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.UpdateAttachmentDetails;

using Core.Models.Attachments.Identifiers;
using Core.Models.Reports.Enums;
using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class UpdateAttachmentDetailsViewModel
{
    public AttachmentId AttachmentId { get; set; }
    public string FileName { get; set; }
    public StudentId StudentId { get; set; }
    public ReportType ReportType { get; set; }

    public SelectList Students { get; set; }
}
