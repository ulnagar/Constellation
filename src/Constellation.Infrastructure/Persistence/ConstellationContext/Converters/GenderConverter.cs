namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.Models.Students.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class GenderConverter : ValueConverter<Gender, string?>
{
    public GenderConverter()
        : base(
            gender => GenderToString(gender),
            value => StringToGender(value),
            new ConverterMappingHints())
    { }

    private static string? GenderToString(Gender? gender) =>
        gender is null
            ? String.Empty
            : gender.Value;

    private static Gender? StringToGender(string value) =>
        string.IsNullOrWhiteSpace(value) 
            ? null 
            : Gender.FromValue(value);

    public override bool ConvertsNulls => true;
}