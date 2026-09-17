namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class GradeConverter : ValueConverter<Grade, string?>
{
    public GradeConverter()
        : base(
            grade => GradeToString(grade),
            value => StringToGrade(value),
            new ConverterMappingHints())
    { }

    private static string? GradeToString(Grade grade) =>
        grade == Grade.Empty ? null : grade.Value;

    private static Grade StringToGrade(string? value)
    {
        if (value is null)
            return Grade.Empty;


        Grade? grade = Grade.FromValue(value);

        return grade ?? Grade.Empty;
    }

    public override bool ConvertsNulls => true;
}