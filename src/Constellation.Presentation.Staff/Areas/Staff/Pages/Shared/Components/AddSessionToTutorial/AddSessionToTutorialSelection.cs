namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddSessionToTutorial;

using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Enums;
using Core.Models.Tutorials.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class AddSessionToTutorialSelection
{
    public TutorialId TutorialId { get; set; }
    [Required]
    [ModelBinder(typeof(IntEnumBinder))]
    public PeriodWeek Week { get; set; }
    [Required]
    [ModelBinder(typeof(IntEnumBinder))]
    public PeriodDay Day { get; set; }
    [Required]
    [DataType(DataType.Time)] 
    public TimeSpan StartTime { get; set; } = new(12, 0, 0);
    [Required]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; } = new(12, 0, 0);
    [Required]
    public StaffId StaffId { get; set; }

    public Dictionary<int, string> Weeks { get; set; } = [];
    public Dictionary<int, string> Days { get; set; } = [];
    public Dictionary<StaffId, string> Staff { get; set; } = new();
}
