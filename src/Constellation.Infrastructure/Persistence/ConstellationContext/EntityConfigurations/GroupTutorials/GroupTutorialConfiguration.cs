namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
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
            .HasMany(tutorial => tutorial.Rolls)
            .WithOne()
            .HasForeignKey(roll => roll.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(tutorial => tutorial.Enrolments)
            .WithOne()
            .HasForeignKey(e => e.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(tutorial => tutorial.StartDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(tutorial => tutorial.EndDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Ignore(tutorial => tutorial.CurrentEnrolments);
    }
}
