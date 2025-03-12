namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Models.SchoolContacts.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;


public sealed class PositionEnumBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(Position))
            return new BinderTypeModelBinder(typeof(BaseFromValueBinder));

        return null;
    }
}