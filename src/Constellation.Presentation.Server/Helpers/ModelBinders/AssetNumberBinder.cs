#nullable enable
namespace Constellation.Presentation.Server.Helpers.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Reflection;

public sealed class AssetNumberBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string modelName = bindingContext.ModelName;
        Type modelType = bindingContext.ModelType;

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

        object? assetNumber = TryParse(modelType, valueProviderResult.FirstValue);
        StoreResult(bindingContext, modelName, assetNumber);

        return Task.CompletedTask;
    }

    private void StoreResult(ModelBindingContext bindingContext, string modelName, object? assetNumber)
    {
        if (assetNumber is not null)
            bindingContext.Result = ModelBindingResult.Success(assetNumber);
        else
            bindingContext.ModelState.TryAddModelError(modelName, "Invalid Asset Number");
    }

    private object? TryParse(Type assetNumberType, string? rawValue) =>
        FromString(assetNumberType, rawValue) is object parsedValue
            //? Activator.CreateInstance(assetNumberType, parsedValue) // For Public Constructors Only
            ? assetNumberType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null)?.Invoke(new object[] { parsedValue }) // For Private Constructors with Parameters
            : null;

    private object? FromString(Type assetNumberType, string? rawValue) =>
        rawValue is string && !string.IsNullOrWhiteSpace(rawValue) && GetContainedType(assetNumberType) is Type containedType
            ? TypeDescriptor.GetConverter(containedType).ConvertFromString(rawValue)
            : null;

    private Type? GetContainedType(Type assetNumberType)
    {
        PropertyInfo? property = assetNumberType.GetProperty("Number");

        if (property is null)
            return null;

        return property.PropertyType;
    }
}