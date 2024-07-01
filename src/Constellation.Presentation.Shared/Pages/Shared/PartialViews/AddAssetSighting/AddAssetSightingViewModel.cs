namespace Constellation.Presentation.Shared.Pages.Shared.PartialViews.AddAssetSighting;

using Core.Models.Assets.ValueObjects;
using Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public sealed class AddAssetSightingViewModel
{
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber AssetNumber { get; set; }
    public string StaffId { get; set; }
    [DataType(DataType.DateTime)] //, DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
    public DateTime SightedAt { get; set; }

    public string Note { get; set; }

    public SelectList StaffList { get; set; }
}
