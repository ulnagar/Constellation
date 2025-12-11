namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Models.EmergencyConsole.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class RecipientGroupBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(RecipientGroup))
            return new BinderTypeModelBinder(typeof(BaseFromValueBinder));

        return null;
    }
}