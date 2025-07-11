﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training;

using Core.Models.StaffMembers;
using Core.Models.Training;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingCompletionConfiguration : IEntityTypeConfiguration<TrainingCompletion>
{
    public void Configure(EntityTypeBuilder<TrainingCompletion> builder)
    {
        builder.ToTable("Completions", "Training");

        builder
            .HasKey(completion => completion.Id);

        builder
            .Property(completion => completion.Id)
            .HasConversion(
                recordId => recordId.Value,
                value => TrainingCompletionId.FromValue(value));

        builder
            .HasOne<StaffMember>()
            .WithMany()
            .HasForeignKey(completion => completion.StaffId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}