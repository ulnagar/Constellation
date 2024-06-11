#nullable enable
namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Reflection;

public sealed class StringEnumerableBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string modelName = bindingContext.ModelName;
        Type modelType = bindingContext.ModelType;

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

        object? enumerable = TryParse(modelType, valueProviderResult.FirstValue);
        StoreResult(bindingContext, modelName, enumerable);

        return Task.CompletedTask;
    }

    private void StoreResult(ModelBindingContext bindingContext, string modelName, object? enumerable)
    {
        if (enumerable is not null)
            bindingContext.Result = ModelBindingResult.Success(enumerable);
        else
            bindingContext.ModelState.TryAddModelError(modelName, "Invalid Enumerable Value");
    }

    private object? TryParse(Type enumerableType, string? rawValue) =>
        FromString(enumerableType, rawValue) is object parsedValue
            //? Activator.CreateInstance(assetNumberType, parsedValue) // For Public Constructors Only
            ? enumerableType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null)?.Invoke(new object[] { parsedValue }) // For Private Constructors with Parameters
            : null;

    private object? FromString(Type enumerableType, string? rawValue) =>
        rawValue is string && !string.IsNullOrWhiteSpace(rawValue) && GetContainedType(enumerableType) is Type containedType
            ? TypeDescriptor.GetConverter(containedType).ConvertFromString(rawValue)
            : null;

    private Type? GetContainedType(Type enumerableType)
    {
        PropertyInfo? property = enumerableType.GetProperty("Value");

        if (property is null)
            return null;

        return property.PropertyType;
    }
}