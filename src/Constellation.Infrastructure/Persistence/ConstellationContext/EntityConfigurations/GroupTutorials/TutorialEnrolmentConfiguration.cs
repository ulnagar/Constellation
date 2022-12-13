namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialEnrolmentConfiguration : IEntityTypeConfiguration<TutorialEnrolment>
{
    public void Configure(EntityTypeBuilder<TutorialEnrolment> builder)
    {
        builder.ToTable("GroupTutorials_Enrolment");

        builder.HasKey(e => e.Id);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GroupTutorial>()
            .WithMany()
            .HasForeignKey(e => e.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
