namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Application.Domains.Auth.Models;
using Core.Models.Auth.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Primitives;
using System;

public class NotificationSettingModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
            throw new ArgumentNullException(nameof(bindingContext));

        string modelName = bindingContext.ModelName;

        string typeFieldName = ModelNames.CreatePropertyModelName(modelName, nameof(NotificationSetting.Type));
        string enabledFieldName = ModelNames.CreatePropertyModelName(modelName, nameof(NotificationSetting.Enabled));

        ValueProviderResult typeResult = bindingContext.ValueProvider.GetValue(typeFieldName);
        ValueProviderResult enabledResult = bindingContext.ValueProvider.GetValue(enabledFieldName);

        // Nothing posted for this entry at all - let the framework treat it as absent
        // rather than reporting a spurious binding failure.
        if (typeResult == ValueProviderResult.None && enabledResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(typeFieldName, typeResult);
        bindingContext.ModelState.SetModelValue(enabledFieldName, enabledResult);

        string? typeValue = typeResult.FirstValue;

        if (string.IsNullOrWhiteSpace(typeValue))
        {
            bindingContext.ModelState.TryAddModelError(typeFieldName, "Notification type is required.");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        NotificationType? notificationType = NotificationType.FromValue(typeValue);

        if (notificationType is null)
        {
            bindingContext.ModelState.TryAddModelError(
                typeFieldName,
                $"'{typeValue}' is not a valid notification type.");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        // Handles the ASP.NET checkbox tag helper's "true,false" hidden-input pattern
        // as well as a plain "true"/"false"/"on" value.
        bool enabled = false;
        if (enabledResult != ValueProviderResult.None)
        {
            StringValues rawValues = enabledResult.Values;
            enabled = rawValues.Any(v =>
                bool.TryParse(v, out bool parsed) && parsed
                || string.Equals(v, "on", StringComparison.OrdinalIgnoreCase));
        }

        //if (!enabled)
        //{
        //    bindingContext.Result = ModelBindingResult.Failed();
        //    return Task.CompletedTask;
        //}

        NotificationSetting model = new()
        {
            Type = notificationType,
            Enabled = enabled,
        };

        bindingContext.Result = ModelBindingResult.Success(model);
        return Task.CompletedTask;
    }
}

public class NotificationSettingModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        return context.Metadata.ModelType == typeof(NotificationSetting)
            ? new BinderTypeModelBinder(typeof(NotificationSettingModelBinder))
            : null;
    }
}