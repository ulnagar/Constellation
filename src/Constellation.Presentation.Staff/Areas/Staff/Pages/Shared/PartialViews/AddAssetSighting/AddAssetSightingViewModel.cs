namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.AddAssetSighting;

using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public sealed class AddAssetSightingViewModel
{
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber AssetNumber { get; set; } = AssetNumber.Empty;

    public string StaffId { get; set; } = string.Empty;

    [DataType(DataType.DateTime)] //, DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime SightedAt { get; set; }

    public string Note { get; set; } = string.Empty;

    public SelectList? StaffList { get; set; }
}
