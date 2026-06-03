namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class MailingAddressConverter : ValueConverter<MailingAddress, string?>
{
    public MailingAddressConverter()
        : base(
            address => MailingAddressToString(address),
            value => StringToMailingAddress(value),
            new ConverterMappingHints())
    { }

    private static string? MailingAddressToString(MailingAddress? address) =>
        address is null
            ? null
            : $"{address.Title}||{address.Line1}||{address.Line2}||{address.Town}||{address.State}||{address.PostCode}";

    private static MailingAddress? StringToMailingAddress(string? value)
    {
        if (value is null)
            return null;

        var parts = value.Split("||");

        return MailingAddress.FromValue(
            parts[0],
            parts[1],
            parts[2],
            parts[3],
            parts[4],
            parts[5]);
    }
}