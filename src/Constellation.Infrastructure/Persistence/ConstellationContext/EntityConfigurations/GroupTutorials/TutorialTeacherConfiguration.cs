namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialTeacherConfiguration : IEntityTypeConfiguration<TutorialTeacher>
{
    public void Configure(EntityTypeBuilder<TutorialTeacher> builder)
    {
        builder.ToTable("GroupTutorials_Teachers");

        builder
            .HasKey(teacher => teacher.Id);

        builder
            .Property(teacher => teacher.Id)
            .HasConversion(
                teacherId => teacherId.Value,
                value => TutorialTeacherId.FromValue(value));

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(teacher => teacher.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<GroupTutorial>()
            .WithMany(tutorial => tutorial.Teachers)
            .HasForeignKey(teacher => teacher.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(teacher => teacher.EffectiveFrom)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(teacher => teacher.EffectiveTo)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}
