namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class GroupTutorialConfiguration : IEntityTypeConfiguration<GroupTutorial>
{
    public void Configure(EntityTypeBuilder<GroupTutorial> builder)
    {
        builder.ToTable("GroupTutorials_Tutorial");

        builder
            .HasKey(t => t.Id);

        builder
            .HasMany(t => t.Teachers)
            .WithOne()
            .HasForeignKey(t => t.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(t => t.Rolls)
            .WithOne()
            .HasForeignKey(r => r.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(t => t.Enrolments)
            .WithOne()
            .HasForeignKey(e => e.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(t => t.StartDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(t => t.EndDate)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Ignore(t => t.CurrentEnrolments);
    }
}
