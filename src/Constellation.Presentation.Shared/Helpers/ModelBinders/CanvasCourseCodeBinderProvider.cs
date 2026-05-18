namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Models.Canvas.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class CanvasCourseCodeBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(CanvasCourseCode))
            return new BinderTypeModelBinder(typeof(FromValueBinder));

        return null;
    }
}