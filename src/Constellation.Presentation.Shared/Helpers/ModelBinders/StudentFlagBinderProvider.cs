namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Application.Domains.Contacts.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class StudentFlagBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.ModelType == typeof(StudentFlag)
            ? new BinderTypeModelBinder(typeof(FromValueBinder))
            : null;
    }
}