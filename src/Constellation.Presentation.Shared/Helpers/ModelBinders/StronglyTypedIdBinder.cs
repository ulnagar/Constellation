namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;

public class StronglyTypedIdModelBinder<TSelf, TValue> : IModelBinder
    where TSelf : IStronglyTypedId<TSelf, TValue>
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ValueProviderResult valueProviderResult = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            if (bindingContext.IsTopLevelObject)
            {
                bindingContext.Result = ModelBindingResult.Success(TSelf.Empty);
            }

            return Task.CompletedTask;
        }

        string? rawValue = valueProviderResult.FirstValue;
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        if (string.IsNullOrEmpty(rawValue))
        {
            bindingContext.Result = ModelBindingResult.Success(TSelf.Empty);
            return Task.CompletedTask;
        }

        try
        {
            // Use TypeConverter so Guid, string, int etc. all work without
            // special-casing — ConvertFromInvariantString handles culture safely.
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TValue));
            TValue typedValue = (TValue)converter.ConvertFromInvariantString(rawValue)!;

            bindingContext.Result = ModelBindingResult.Success(TSelf.FromValue(typedValue));
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
        }

        return Task.CompletedTask;
    }
}

public class StronglyTypedIdBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        Type modelType = context.Metadata.ModelType;

        // Walk the interface list looking for IStronglyTypedId<TSelf, TValue>
        Type? matchingInterface = modelType
            .GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IStronglyTypedId<,>));

        if (matchingInterface is null)
            return null;

        // [0] = TSelf (the concrete type), [1] = TValue (Guid, string, etc.)
        Type[] typeArgs = matchingInterface.GetGenericArguments();

        Type binderType = typeof(StronglyTypedIdModelBinder<,>)
            .MakeGenericType(typeArgs);

        return (IModelBinder)Activator.CreateInstance(binderType)!;
    }
}