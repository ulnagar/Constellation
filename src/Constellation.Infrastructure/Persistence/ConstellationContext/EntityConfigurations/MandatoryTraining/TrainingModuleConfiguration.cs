namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.MandatoryTraining;

using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Models.MandatoryTraining.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingModuleConfiguration : IEntityTypeConfiguration<TrainingModule>
{
    public void Configure(EntityTypeBuilder<TrainingModule> builder)
    {
        builder.ToTable("MandatoryTraining_Modules");

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
            .HasMany(module => module.Roles)
            .WithOne(role => role.Module)
            .HasForeignKey(role => role.ModuleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .Navigation(module => module.Completions)
            .AutoInclude();

        builder
            .Navigation(module => module.Roles)
            .AutoInclude();
    }
}
