namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Awards;

using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.Students;
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
            .HasOne<StaffMember>()
            .WithMany()
            .HasForeignKey(award => award.TeacherId);

        builder
            .HasIndex(award => award.IncidentId);
    }
}
