namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Converters;

using Core.Models.EnrolmentContext.Offer.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class ProgramConverter : ValueConverter<Program, string?>
{
    public ProgramConverter()
        : base(
            program => ProgramToString(program),
            value => StringToProgram(value),
            new ConverterMappingHints())
    { }

    private static string? ProgramToString(Program? program) =>
        program?.Value;

    private static Program StringToProgram(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Program.Empty
            : Program.FromValue(value);

    public override bool ConvertsNulls => true;
}