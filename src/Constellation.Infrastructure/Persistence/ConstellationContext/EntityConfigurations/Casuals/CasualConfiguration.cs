namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Casuals;

using Constellation.Core.Models;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Identifiers;
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
            .HasOne<School>()
            .WithMany()
            .HasForeignKey(casual => casual.SchoolCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(casual => casual.EmailAddress)
            .IsUnique();
    }
}