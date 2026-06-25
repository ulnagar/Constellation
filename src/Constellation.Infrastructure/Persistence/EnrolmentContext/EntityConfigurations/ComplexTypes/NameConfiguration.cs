namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.ComplexTypes;

using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class NameConfiguration
{
    public static ComplexPropertyBuilder<Name> ConfigureName(
        this ComplexPropertyBuilder<Name> builder,
        string? columnPrefix = null)
    {
        string Col(string name) => columnPrefix is null ? name : $"{columnPrefix}_{name}";
        
        builder
            .Property(name => name.FirstName)
            .HasColumnName(Col(nameof(Name.FirstName)))
            .IsRequired();

        builder
            .Property(name => name.PreferredName)
            .HasColumnName(Col(nameof(Name.PreferredName)))
            .IsRequired(false);

        builder
            .Property(name => name.LastName)
            .HasColumnName(Col(nameof(Name.LastName)))
            .IsRequired();

        return builder;
    }

    public static OwnedNavigationBuilder<TOwner, Name> ConfigureName<TOwner>(
        this OwnedNavigationBuilder<TOwner, Name> builder,
        string? columnPrefix = null)
        where TOwner : class
    {
        string Col(string name) => columnPrefix is null ? name : $"{columnPrefix}_{name}";

        builder
            .Property(name => name.FirstName)
            .HasColumnName(Col(nameof(Name.FirstName)))
            .IsRequired();

        builder
            .Property(name => name.PreferredName)
            .HasColumnName(Col(nameof(Name.PreferredName)))
            .IsRequired(false);

        builder
            .Property(name => name.LastName)
            .HasColumnName(Col(nameof(Name.LastName)))
            .IsRequired();

        return builder;
    }
}
