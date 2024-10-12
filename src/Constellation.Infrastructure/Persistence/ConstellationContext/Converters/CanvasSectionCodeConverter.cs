namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.Models.Canvas.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class CanvasSectionCodeConverter : ValueConverter<CanvasSectionCode, string?>
{
    public CanvasSectionCodeConverter()
        : base(
            code => CanvasSectionCodeToString(code),
            value => StringToCanvasSectionCode(value),
            new ConverterMappingHints()) 
    { }

    private static string? CanvasSectionCodeToString(CanvasSectionCode code) =>
        code == CanvasSectionCode.Empty ? null : code.ToString();

    private static CanvasSectionCode StringToCanvasSectionCode(string? value) =>
        value == null ? CanvasSectionCode.Empty : CanvasSectionCode.FromValue(value);

    public override bool ConvertsNulls => true;
}