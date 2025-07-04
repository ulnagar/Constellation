namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Casuals;

using Constellation.Core.Models;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Identifiers;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class CasualConfiguration : IEntityTypeConfiguration<Casual>
{
    public void Configure(EntityTypeBuilder<Casual> builder)
    {
        builder.ToTable("Casuals_Casuals");

        builder
            .HasKey(casual => casual.Id);

        builder
            .Property(casual => casual.Id)
            .HasConversion(
                casualId => casualId.Value,
                value => CasualId.FromValue(value));

        builder
            .ComplexProperty(casual => casual.Name)
            .IsRequired();

        builder
            .ComplexProperty(casual => casual.Name)
            .Property(name => name.FirstName)
            .HasColumnName(nameof(Name.FirstName))
            .IsRequired();

        builder
            .ComplexProperty(casual => casual.Name)
            .Property(name => name.PreferredName)
            .HasColumnName(nameof(Name.PreferredName))
            .IsRequired(false);

        builder
            .ComplexProperty(casual => casual.Name)
            .Property(name => name.LastName)
            .HasColumnName(nameof(Name.LastName))
            .IsRequired();

        builder
            .Property(member => member.EmailAddress)
            .HasConversion(
                email => email.Email,
                value => EmailAddress.FromValue(value));

        builder
            .HasOne<School>()
            .WithMany()
            .HasForeignKey(casual => casual.SchoolCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(casual => casual.EmailAddress)
            .IsUnique();
    }
}