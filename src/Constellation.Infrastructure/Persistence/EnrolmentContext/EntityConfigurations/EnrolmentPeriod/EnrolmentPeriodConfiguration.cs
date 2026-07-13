namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.EnrolmentPeriod;

using Converters;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EnrolmentPeriodConfiguration : IEntityTypeConfiguration<EnrolmentPeriod>
{
    public void Configure(EntityTypeBuilder<EnrolmentPeriod> builder)
    {
        builder.ToTable("Periods");

        builder
            .HasKey(entry => entry.Id);

        builder
            .Property(entry => entry.Program)
            .HasConversion<ProgramConverter>();
    }
}