namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Constellation.Core.Primitives;
using Core.Enums;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class GradeConverter : ValueConverter<Grade, string>
{
    public GradeConverter()
        : base(
            grade => GradeToString(grade),
            value => StringToGrade(value),
            new ConverterMappingHints())
    { }

    private static string GradeToString(Grade grade) =>
        grade == Grade.Empty ? string.Empty : grade.Value;

    private static Grade StringToGrade(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Grade.Empty;

        Grade? grade = Grade.FromValue(value);

        return grade ?? Grade.Empty;
    }
}

internal sealed class GradeConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (IConventionEntityType entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (IConventionProperty property in entityType.GetProperties())
            {
                if (property.ClrType != typeof(Grade))
                    continue;

                property.Builder.HasConversion(typeof(GradeConverter));
            }
        }
    }
}