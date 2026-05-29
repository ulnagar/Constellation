namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.LinkAssessmentToCanvas;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class LinkAssessmentToCanvasSelection
{
    public required List<SelectListItem> Courses { get; init; }

    public string SelectedAssessment { get; set; }

    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly ForwardDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}
