namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialRollStudentConfiguration : IEntityTypeConfiguration<TutorialRollStudent>
{
    public void Configure(EntityTypeBuilder<TutorialRollStudent> builder)
    {
        builder.ToTable("GroupTutorials_RollStudent");

        builder
            .HasKey(rollStudent => new
            {
                rollStudent.TutorialRollId,
                rollStudent.StudentId
            });

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(rollStudent => rollStudent.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<TutorialRoll>()
            .WithMany(roll => roll.Students)
            .HasForeignKey(rollStudent => rollStudent.TutorialRollId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
