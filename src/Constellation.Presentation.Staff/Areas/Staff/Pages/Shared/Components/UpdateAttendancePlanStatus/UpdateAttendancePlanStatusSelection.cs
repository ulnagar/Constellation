namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UpdateAttendancePlanStatus;

using Constellation.Core.Models.Attendance.Enums;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class UpdateAttendancePlanStatusSelection
{
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AttendancePlanStatus CurrentStatus { get; set; }
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AttendancePlanStatus SelectedStatus { get; set; }

    public SelectList StatusList => new(AttendancePlanStatus.GetOptions, "Value", "Name");
}
