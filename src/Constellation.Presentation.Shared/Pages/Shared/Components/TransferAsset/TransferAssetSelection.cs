namespace Constellation.Presentation.Shared.Pages.Shared.Components.TransferAsset;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public sealed class TransferAssetSelection
{
    [ModelBinder(typeof(StringEnumerableBinder))]
    public LocationCategory LocationCategory { get; set; }

    public string Site { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly ArrivalDate { get; set; }


    public SelectList LocationCategoryList => new(LocationCategory.GetOptions, "Value", "Name");
    public SelectList SchoolList { get; set; }
}
