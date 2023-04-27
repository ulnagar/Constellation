namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentAwardConfiguration : IEntityTypeConfiguration<StudentAward>
{
    public void Configure(EntityTypeBuilder<StudentAward> builder)
    {
        builder.ToTable("Awards_StudentAwards");

        builder
            .HasKey(award => award.Id);

        builder
            .Property(award => award.Id)
            .HasConversion(
                awardId => awardId.Value,
                value => StudentAwardId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(award => award.StudentId);

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(award => award.TeacherId);

        builder
            .HasIndex(award => award.IncidentId);
    }
}
