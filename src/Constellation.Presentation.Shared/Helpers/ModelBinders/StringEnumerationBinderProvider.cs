namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class StringEnumerationBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsStringEnumeration(context.Metadata.ModelType))
            return new BinderTypeModelBinder(typeof(BaseFromValueBinder));

        return null;
    }

    private static bool IsStringEnumeration(Type type)
    {
        var current = type;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(StringEnumeration<>))
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }
}