namespace Constellation.Presentation.Shared.Pages.Shared.Components.AllocateAsset;

using Core.Models.Assets.Enums;
using Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class AllocateDeviceSelection
{
    [ModelBinder(typeof(StringEnumerableBinder))]
    public AllocationType AllocationType { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;


    public SelectList AllocationTypeList => new(AllocationType.GetOptions, "Value", "Name");
    public SelectList StudentList { get; set; }
    public SelectList StaffList { get; set; }
    public SelectList SchoolList { get; set; }
}
