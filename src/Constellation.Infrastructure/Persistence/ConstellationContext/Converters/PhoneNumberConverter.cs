#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.ValueObjects;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

internal sealed class PhoneNumberConverter : ValueConverter<PhoneNumber, string?>
{
    public PhoneNumberConverter()
        : base(
            pn => PhoneNumberToString(pn),
            value => StringToPhoneNumber(value),
            new ConverterMappingHints())
    { }

    private static string? PhoneNumberToString(PhoneNumber? number) =>
        number is null ? null
        : number == PhoneNumber.Empty ? null 
        : number.ToString(PhoneNumber.Format.None);

    private static PhoneNumber StringToPhoneNumber(string? value) =>
        value == null ? PhoneNumber.Empty 
        : string.IsNullOrWhiteSpace(value) ? PhoneNumber.Empty
        : PhoneNumber.FromValue(value);

    public override bool ConvertsNulls => true;
}