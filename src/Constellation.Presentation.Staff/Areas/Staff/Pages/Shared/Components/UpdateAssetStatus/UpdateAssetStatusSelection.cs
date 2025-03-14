﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UpdateAssetStatus;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class UpdateAssetStatusSelection
{
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AssetStatus CurrentStatus { get; set; }
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AssetStatus SelectedStatus { get; set; }

    public SelectList StatusList => new(AssetStatus.GetOptions, "Value", "Name");
}
