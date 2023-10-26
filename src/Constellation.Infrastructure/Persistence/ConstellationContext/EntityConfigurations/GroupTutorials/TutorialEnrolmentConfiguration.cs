namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models.GroupTutorials;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialEnrolmentConfiguration : IEntityTypeConfiguration<TutorialEnrolment>
{
    public void Configure(EntityTypeBuilder<TutorialEnrolment> builder)
    {
        builder.ToTable("GroupTutorials_Enrolment");

        builder
            .HasKey(e => e.Id);

        builder
            .Property(enrol => enrol.Id)
            .HasConversion(
                enrolId => enrolId.Value,
                value => TutorialEnrolmentId.FromValue(value));

        builder
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(enrol => enrol.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<GroupTutorial>()
            .WithMany(t => t.Enrolments)
            .HasForeignKey(e => e.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(e => e.EffectiveFrom)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(e => e.EffectiveTo)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}
