namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class GroupTutorialConfiguration : IEntityTypeConfiguration<GroupTutorial>
{
    public void Configure(EntityTypeBuilder<GroupTutorial> builder)
    {
        builder.ToTable("GroupTutorials_Tutorial");

        builder
            .HasKey(tutorial => tutorial.Id);

        builder
            .Property(tutorial => tutorial.Id)
            .HasConversion(
                tutorialId => tutorialId.Value,
                value => GroupTutorialId.FromValue(value));

        builder
            .HasMany(tutorial => tutorial.Teachers)
            .WithOne()
            .HasForeignKey(tutorial => tutorial.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(tutorial => tutorial.Teachers)
            .AutoInclude();

        builder
            .HasMany(tutorial => tutorial.Rolls)
            .WithOne()
            .HasForeignKey(roll => roll.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(tutorial => tutorial.Rolls)
            .AutoInclude();

        builder
            .HasMany(tutorial => tutorial.Enrolments)
            .WithOne()
            .HasForeignKey(e => e.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Navigation(tutorial => tutorial.Enrolments)
            .AutoInclude();

        builder
            .Ignore(tutorial => tutorial.CurrentEnrolments);

        builder
            .Navigation(tutorial => tutorial.Enrolments)
            .AutoInclude();

        builder
            .Navigation(tutorial => tutorial.Teachers)
            .AutoInclude();

        builder
            .Navigation(tutorial => tutorial.Rolls)
            .AutoInclude();
    }
}
