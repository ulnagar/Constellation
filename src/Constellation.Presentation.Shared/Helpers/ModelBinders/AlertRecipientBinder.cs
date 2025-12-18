namespace Constellation.Presentation.Shared.Helpers.ModelBinders;

using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

public sealed class AlertRecipientBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        string modelName = bindingContext.ModelName;
        Type modelType = bindingContext.ModelType;

        ValueProviderResult firstNameResult = bindingContext.ValueProvider.GetValue($"{modelName}.Name.FirstName");
        ValueProviderResult preferredNameResult = bindingContext.ValueProvider.GetValue($"{modelName}.Name.PreferredName");
        ValueProviderResult lastNameResult = bindingContext.ValueProvider.GetValue($"{modelName}.Name.LastName");

        if (firstNameResult == ValueProviderResult.None &&
            preferredNameResult == ValueProviderResult.None &&
            lastNameResult == ValueProviderResult.None)
            return Task.CompletedTask;

        Name? name = null;
        EmailAddress emailAddress = EmailAddress.None;
        PhoneNumber phoneNumber = PhoneNumber.Empty;

        if (firstNameResult != ValueProviderResult.None &&
            preferredNameResult != ValueProviderResult.None &&
            lastNameResult != ValueProviderResult.None)
        {
            Result<Name> nameResult = Name.Create(firstNameResult.FirstValue, preferredNameResult.FirstValue, lastNameResult.FirstValue);

            if (nameResult.IsFailure)
                return Task.CompletedTask;

            name = nameResult.Value;
        }

        if (firstNameResult != ValueProviderResult.None &&
            lastNameResult != ValueProviderResult.None)
        {
            Result<Name> nameResult = Name.Create(firstNameResult.FirstValue, string.Empty, lastNameResult.FirstValue);

            if (nameResult.IsFailure)
                return Task.CompletedTask;

            name = nameResult.Value;
        }

        if (name is null)
            return Task.CompletedTask;

        ValueProviderResult emailAddressResult = bindingContext.ValueProvider.GetValue($"{modelName}.EmailAddress");
        ValueProviderResult phoneNumberResult = bindingContext.ValueProvider.GetValue($"{modelName}.PhoneNumber");

        if (emailAddressResult != ValueProviderResult.None)
        {
            Result<EmailAddress> email = EmailAddress.Create(emailAddressResult.FirstValue);

            if (email.IsSuccess)
                emailAddress = email.Value;
        }

        if (phoneNumberResult != ValueProviderResult.None)
        {
            Result<PhoneNumber> phone = PhoneNumber.Create(phoneNumberResult.FirstValue);

            if (phone.IsSuccess)
                phoneNumber = phone.Value;
        }

        AlertRecipient alertRecipient = AlertRecipient.Create(name, emailAddress, phoneNumber);

        bindingContext.Result = ModelBindingResult.Success(alertRecipient);

        return Task.CompletedTask;
    }
}

public sealed class AlertRecipientBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ModelType == typeof(AlertRecipient))
            return new BinderTypeModelBinder(typeof(AlertRecipientBinder));

        return null;
    }
}