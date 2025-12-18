namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class EmailAddressConverter : ValueConverter<EmailAddress, string?>
{
    public EmailAddressConverter()
        : base(
            email => EmailAddressToString(email),
            value => StringToEmailAddress(value),
            new ConverterMappingHints())
    { }

    private static string? EmailAddressToString(EmailAddress? email) =>
        email is null ? null
        : email == EmailAddress.None ? null
        : email.Email;

    private static EmailAddress StringToEmailAddress(string? value) =>
        value == null ? EmailAddress.None : EmailAddress.FromValue(value);

    public override bool ConvertsNulls => true;
}