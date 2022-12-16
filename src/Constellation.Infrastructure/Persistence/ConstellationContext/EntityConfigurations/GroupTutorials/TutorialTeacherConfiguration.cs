﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.GroupTutorials;

using Constellation.Core.Models;
using Constellation.Core.Models.GroupTutorials;
using Constellation.Infrastructure.Persistence.ConstellationContext.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TutorialTeacherConfiguration : IEntityTypeConfiguration<TutorialTeacher>
{
    public void Configure(EntityTypeBuilder<TutorialTeacher> builder)
    {
        builder.ToTable("GroupTutorials_Teachers");

        builder.HasKey(t => t.Id);

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(t => t.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<GroupTutorial>()
            .WithMany(g => g.Teachers)
            .HasForeignKey(t => t.TutorialId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(e => e.EffectiveFrom)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();

        builder
            .Property(e => e.EffectiveTo)
            .HasConversion<DateOnlyConverter, DateOnlyComparer>();
    }
}
