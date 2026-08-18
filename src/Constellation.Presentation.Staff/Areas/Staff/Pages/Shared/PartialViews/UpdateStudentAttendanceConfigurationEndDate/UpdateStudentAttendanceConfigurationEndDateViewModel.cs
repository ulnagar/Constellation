namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.UpdateStudentAttendanceConfigurationEndDate;

using Core.Models.Absences.Enums;
using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;
using System;
using System.ComponentModel.DataAnnotations;

public sealed class UpdateStudentAttendanceConfigurationEndDateViewModel
{
    public string ViewName = "UpdateStudentAttendanceConfigurationEndDate";

    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public required StudentId StudentId { get; set; }
    [ModelBinder(typeof(BaseFromValueBinder))]
    public required AbsenceType Type { get; set; }
}
