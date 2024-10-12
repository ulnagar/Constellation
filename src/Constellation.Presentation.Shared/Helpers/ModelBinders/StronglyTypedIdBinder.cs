#nullable enable
namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.ComponentModel;

public sealed class StronglyTypedIdBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string modelName = bindingContext.ModelName;
        Type modelType = bindingContext.ModelType;

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

        object? id = TryParse(modelType, valueProviderResult.FirstValue);
        StoreResult(bindingContext, modelName, id);

        return Task.CompletedTask;
    }

    private void StoreResult(ModelBindingContext bindingContext, string modelName, object? id)
    {
        if (id is not null)
            bindingContext.Result = ModelBindingResult.Success(id);
        else
            bindingContext.ModelState.TryAddModelError(modelName, "Invalid ID");
    }

    private object? TryParse(Type idType, string? rawValue) =>
        FromString(idType, rawValue) is object parsedValue
            ? Activator.CreateInstance(idType, parsedValue)
            : null;

    private object? FromString(Type idType, string? rawValue) =>
        rawValue is string && !string.IsNullOrWhiteSpace(rawValue) && GetContainedType(idType) is Type containedType
            ? TypeDescriptor.GetConverter(containedType).ConvertFromString(rawValue)
            : null;

    private Type? GetContainedType(Type idType) =>
        idType.GetProperty("Value")?.PropertyType;
}

public sealed class StronglyTypedIdBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Type[] interfaceTypes = context.Metadata.ModelType.FindInterfaces((type, criteria) => type.ToString() == criteria.ToString(), typeof(IStronglyTypedId));

        if (interfaceTypes.Contains(typeof(IStronglyTypedId)))
            return new BinderTypeModelBinder(typeof(StronglyTypedIdBinder));

        return null;
    }
}