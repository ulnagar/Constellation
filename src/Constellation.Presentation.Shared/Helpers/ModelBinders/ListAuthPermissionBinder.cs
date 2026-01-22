namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Application.Models.Auth;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public sealed class ListAuthPermissionBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        IValueProvider valueProvider = bindingContext.ValueProvider;
        string? valueFromRequest = valueProvider.GetValue(bindingContext.ModelName).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(valueFromRequest))
            return Task.CompletedTask;

        List<AuthPermission> list = [];

        string[] items = valueFromRequest.Split(';');
        foreach (string item in items)
        {
            AuthPermission? permission = AuthPermission.FromValue(item);

            if (permission is null)
                continue;

            list.Add(permission);
        }

        bindingContext.Result = ModelBindingResult.Success(list);
        return Task.CompletedTask;
    }
}