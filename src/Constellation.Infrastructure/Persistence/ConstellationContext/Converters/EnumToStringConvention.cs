namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class EnumToStringConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder builder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in builder.Metadata.GetEntityTypes())
        {
            ApplyToProperties(entityType.GetProperties());

            // Complex types (e.g. ApplicationState) need separate handling
            foreach (var complexProperty in entityType.GetComplexProperties())
                ApplyToComplexType(complexProperty.ComplexType);
        }
    }

    private static void ApplyToComplexType(IConventionComplexType complexType)
    {
        ApplyToProperties(complexType.GetProperties());

        // Recurse in case of nested complex types
        foreach (var nested in complexType.GetComplexProperties())
            ApplyToComplexType(nested.ComplexType);
    }

    private static void ApplyToProperties(IEnumerable<IConventionProperty> properties)
    {
        foreach (var property in properties)
        {
            var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

            if (type.IsEnum)
            {
                int maxLength = Enum.GetNames(type).Max(n => n.Length);
                property.Builder.HasConversion(typeof(string));
                property.Builder.HasMaxLength(maxLength);
            }
        }
    }
}