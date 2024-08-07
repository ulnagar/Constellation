﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training;

using Core.Models.Training;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingModuleConfiguration : IEntityTypeConfiguration<TrainingModule>
{
    public void Configure(EntityTypeBuilder<TrainingModule> builder)
    {
        builder.ToTable("Modules", "Training");

        builder
            .HasKey(module => module.Id);

        builder
            .Property(module => module.Id)
            .HasConversion(
                moduleId => moduleId.Value,
                value => TrainingModuleId.FromValue(value));

        builder
            .HasMany(module => module.Completions)
            .WithOne(completion => completion.Module)
            .HasForeignKey(completion => completion.TrainingModuleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasMany(module => module.Assignees)
            .WithOne()
            .HasForeignKey(role => role.ModuleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .Navigation(module => module.Completions)
            .AutoInclude();

        builder
            .Navigation(module => module.Assignees)
            .AutoInclude();
    }
}
