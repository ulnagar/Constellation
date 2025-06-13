namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AllocateAsset;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class AllocateDeviceSelection
{
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AllocationType AllocationType { get; set; }

    public StudentId StudentId { get; set; } = StudentId.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public StaffId StaffId { get; set; } = StaffId.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;


    public SelectList AllocationTypeList => new(AllocationType.GetOptions, "Value", "Name");
    public SelectList StudentList { get; set; }
    public SelectList StaffList { get; set; }
    public SelectList SchoolList { get; set; }
}
