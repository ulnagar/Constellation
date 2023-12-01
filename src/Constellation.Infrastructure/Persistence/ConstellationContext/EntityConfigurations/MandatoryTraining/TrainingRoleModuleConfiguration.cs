namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.MandatoryTraining;

using Core.Models.MandatoryTraining;
using Core.Models.MandatoryTraining.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingRoleModuleConfiguration : IEntityTypeConfiguration<TrainingRoleModule>
{
    public void Configure(EntityTypeBuilder<TrainingRoleModule> builder)
    {
        builder.ToTable("MandatoryTraining_RoleModules");

        builder
            .HasKey(module => new { module.RoleId, module.ModuleId });

        builder
            .Property(module => module.RoleId)
            .HasConversion(
                id => id.Value,
                value => TrainingRoleId.FromValue(value));

        builder
            .Property(module => module.ModuleId)
            .HasConversion(
                id => id.Value,
                value => TrainingModuleId.FromValue(value));

        builder
            .HasOne(module => module.Role)
            .WithMany(role => role.Modules)
            .HasForeignKey(module => module.RoleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasOne(module => module.Module)
            .WithMany(module => module.Roles)
            .HasForeignKey(module => module.ModuleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .Navigation(module => module.Role)
            .AutoInclude();

        builder
            .Navigation(module => module.Module)
            .AutoInclude();
    }
}