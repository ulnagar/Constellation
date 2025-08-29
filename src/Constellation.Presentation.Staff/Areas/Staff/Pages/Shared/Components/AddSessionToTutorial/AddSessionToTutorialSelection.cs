namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddSessionToTutorial;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Identifiers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class AddSessionToTutorialSelection
{
    public TutorialId TutorialId { get; set; }
    [Required]
    public PeriodId PeriodId { get; set; }
    [Required]
    public StaffId StaffId { get; set; }

    public Dictionary<StaffId, string> Staff { get; set; } = new();
    public SelectList Periods { get; set; }
}
