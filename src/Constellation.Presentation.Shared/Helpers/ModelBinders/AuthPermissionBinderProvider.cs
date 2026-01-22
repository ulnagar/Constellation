namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Application.Models.Auth;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class AuthPermissionBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(AuthPermission))
            return new BinderTypeModelBinder(typeof(BaseFromValueBinder));

        return null;
    }
}