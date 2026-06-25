namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class PhoneNumberConverter : ValueConverter<PhoneNumber, string?>
{
    public PhoneNumberConverter()
        : base(
            pn => PhoneNumberToString(pn),
            value => StringToPhoneNumber(value),
            new ConverterMappingHints())
    { }

    private static string? PhoneNumberToString(PhoneNumber? number) =>
        number is null ? string.Empty
        : number == PhoneNumber.Empty ? string.Empty 
        : number.ToString(PhoneNumber.Format.None);

    private static PhoneNumber StringToPhoneNumber(string? value) =>
        value == null ? PhoneNumber.Empty 
        : string.IsNullOrWhiteSpace(value) ? PhoneNumber.Empty
        : PhoneNumber.FromValue(value);

    public override bool ConvertsNulls => true;
}