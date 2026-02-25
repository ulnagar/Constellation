namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Models.AppSettings.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class ContactPositionBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(ContactPosition))
            return new BinderTypeModelBinder(typeof(BaseFromValueBinder));

        return null;
    }
}