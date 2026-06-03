namespace Constellation.Infrastructure.Persistence.ConstellationContext.Extensions;

using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class NameComplexPropertyBuilderExtensions
{
    public static ComplexPropertyBuilder<Name> ApplyConfiguration(
        this ComplexPropertyBuilder<Name> builder)
    {
        builder
            .Property(n => n.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .Property(n => n.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .Property(n => n.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        return builder;
    }
}
