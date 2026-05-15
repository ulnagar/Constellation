namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.UpsertAssessmentInstructions;

using Core.Models.Assessments.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

public sealed class UpsertAssessmentInstructionsViewModel
{
    public UserCategory Category { get; set; }
    public string Instructions { get; set; }
}
