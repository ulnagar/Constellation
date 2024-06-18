namespace Constellation.Presentation.Shared.Pages.Shared.Components.UpdateAssetStatus;

using Core.Models.Assets.Enums;
using Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class UpdateAssetStatusSelection
{
    [ModelBinder(typeof(StringEnumerableBinder))]
    public AssetStatus CurrentStatus { get; set; }
    [ModelBinder(typeof(StringEnumerableBinder))]
    public AssetStatus SelectedStatus { get; set; }

    public SelectList StatusList => new(AssetStatus.GetOptions, "Value", "Name");
}
