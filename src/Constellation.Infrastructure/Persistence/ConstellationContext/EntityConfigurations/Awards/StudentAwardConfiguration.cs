namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students;
using Core.Models.StaffMembers.Identifiers;
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
            .Property(award => award.TeacherId)
            .HasConversion(
                id => id.Value,
                value => StaffId.FromValue(value));

        builder
            .HasIndex(award => award.IncidentId);
    }
}
