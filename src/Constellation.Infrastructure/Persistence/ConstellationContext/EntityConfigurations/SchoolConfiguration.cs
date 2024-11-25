namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("Schools");

        builder.HasKey(s => s.Code);

        builder.Property(s => s.Code)
            .HasMaxLength(4);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(10);

        builder.Property(s => s.FaxNumber)
            .HasMaxLength(10);

        builder.HasMany<SchoolEnrolment>()
            .WithOne()
            .HasForeignKey(s => s.SchoolCode)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(s => s.Staff)
            .WithOne(s => s.School)
            .HasForeignKey(s => s.SchoolCode)
            .OnDelete(DeleteBehavior.NoAction);
    }
}