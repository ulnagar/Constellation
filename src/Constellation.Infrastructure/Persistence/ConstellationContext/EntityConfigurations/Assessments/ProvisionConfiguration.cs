namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assessments;

using Core.Models.Assessments;
using Core.Models.Assessments.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ProvisionConfiguration : IEntityTypeConfiguration<Provision>
{
    public void Configure(EntityTypeBuilder<Provision> builder)
    {
        builder.ToTable("Provisions", "Assessments");

        builder
            .HasKey(provision => provision.Id);

        builder
            .Property(provision => provision.Code)
            .HasConversion(
                code => code.Value,
                value => ProvisionCode.FromValue(value));
    }
}