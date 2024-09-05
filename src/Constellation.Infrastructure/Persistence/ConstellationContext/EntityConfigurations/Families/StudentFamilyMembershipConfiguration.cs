namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Families;

using Constellation.Core.Models.Families;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StudentFamilyMembershipConfiguration : IEntityTypeConfiguration<StudentFamilyMembership>
{
    public void Configure(EntityTypeBuilder<StudentFamilyMembership> builder)
    {
        builder.ToTable("Families_StudentMemberships");

        builder
            .HasKey(s => new { s.FamilyId, s.StudentId });

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(member => member.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Family>()
            .WithMany(family => family.Students)
            .HasForeignKey(member => member.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .OwnsOne(family => family.StudentReferenceNumber);
    }
}
