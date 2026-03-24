namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Constellation.Core.Models.Messaging.Drafts;
using Core.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Collections.Generic;
using System.Text.Json;

public sealed class MessageRecipientListBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (!bindingContext.HttpContext.Request.HasJsonContentType())
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        using var reader = new StreamReader(bindingContext.HttpContext.Request.Body);
        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<RecipientsPayload>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto?.Recipients is null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var recipients = dto.Recipients
                .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .Select(r => CreateRecipient(r))
                .Where(r => r is not null)
                .Cast<MessageRecipient>()
                .ToList();

            bindingContext.Result = ModelBindingResult.Success(recipients);
        }
        catch (JsonException ex)
        {
            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                $"Invalid JSON payload: {ex.Message}");

            bindingContext.Result = ModelBindingResult.Failed();
        }
    }

    private static MessageRecipient? CreateRecipient(RecipientDto dto)
    {
        var hasEmail = !string.IsNullOrWhiteSpace(dto.EmailAddress?.Value);
        var hasPhone = !string.IsNullOrWhiteSpace(dto.PhoneNumber?.Value);

        return (hasEmail, hasPhone) switch
        {
            (true, true) => new MessageRecipient(
                                 EmailAddress.FromValue(dto.EmailAddress!.Value),
                                 PhoneNumber.FromValue(dto.PhoneNumber!.Value),
                                 dto.Name),
            (true, false) => new MessageRecipient(
                                 EmailAddress.FromValue(dto.EmailAddress!.Value),
                                 dto.Name),
            (false, true) => new MessageRecipient(
                                 PhoneNumber.FromValue(dto.PhoneNumber!.Value),
                                 dto.Name),
            _ => null   // neither — skip
        };
    }

    // DTOs used only for deserialisation — keeps domain types clean
    private sealed record RecipientsPayload(List<RecipientDto> Recipients);

    private sealed record RecipientDto(
        string Name,
        ValueObjectDto? EmailAddress,
        ValueObjectDto? PhoneNumber);

    private sealed record ValueObjectDto(string Value);
}

// ModelBinders/MessageRecipientListBinderProvider.cs
public sealed class MessageRecipientListBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(List<MessageRecipient>))
            return new BinderTypeModelBinder(typeof(MessageRecipientListBinder));

        return null;
    }
}