namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Microsoft.AspNetCore.Mvc.ModelBinding;

public sealed class IntEnumBinder : IModelBinder
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

    private object? TryParse(Type enumerableType, string? rawValue)
    {
        if (rawValue is null)
            return null;

        int convertedValue = Convert.ToInt32(rawValue);

        Type? baseType = enumerableType.BaseType;

        object? typedValue = baseType!.GetMethod("FromValue")?.Invoke(null, new object?[] { convertedValue });

        return typedValue;
    }
}