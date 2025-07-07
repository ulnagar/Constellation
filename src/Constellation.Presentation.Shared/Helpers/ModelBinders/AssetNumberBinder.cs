#nullable enable
namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Models.Assets.ValueObjects;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class AssetNumberBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string modelName = bindingContext.ModelName;

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

        AssetNumber? assetNumber = TryParse(valueProviderResult.FirstValue);
        StoreResult(bindingContext, modelName, assetNumber);

        return Task.CompletedTask;
    }

    private static void StoreResult(ModelBindingContext bindingContext, string modelName, AssetNumber? assetNumber)
    {
        if (assetNumber is not null)
            bindingContext.Result = ModelBindingResult.Success(assetNumber);
        else
            bindingContext.ModelState.TryAddModelError(modelName, "Invalid Asset Number");
    }

    private static AssetNumber? TryParse(string? rawValue) =>
        AssetNumber.FromValue(rawValue);
}

public sealed class AssetNumberBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.ModelType == typeof(AssetNumber) 
            ? new BinderTypeModelBinder(typeof(AssetNumberBinder)) 
            : null;
    }
}