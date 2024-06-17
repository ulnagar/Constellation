namespace Constellation.Presentation.Shared.Pages.Shared.Components.AddAssetSighting;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public sealed class AddAssetSightingSelection
{
    public string StaffId { get; set; }
    [DataType(DataType.DateTime)] //, DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime SightedAt { get; set; }

    public string Note { get; set; }

    public SelectList StaffList { get; set; }
}
