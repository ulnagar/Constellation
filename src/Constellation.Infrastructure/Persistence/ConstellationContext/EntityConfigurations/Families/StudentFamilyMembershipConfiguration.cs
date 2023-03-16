namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models.Families;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentFamilyMembershipConfiguration : IEntityTypeConfiguration<StudentFamilyMembership>
{
    public void Configure(EntityTypeBuilder<StudentFamilyMembership> builder)
    {
        builder.ToTable("Families_StudentMemberships");

        builder
            .HasKey(s => new { s.FamilyId, s.StudentId });
    }
}
