namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training.Roles;

using Constellation.Core.Models.Training.Contexts.Roles;
using Core.Models.Training;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingRoleModuleConfiguration : IEntityTypeConfiguration<TrainingRoleModule>
{
    public void Configure(EntityTypeBuilder<TrainingRoleModule> builder)
    {
        builder.ToTable("Training_Roles_Modules");

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
            .HasOne<TrainingModule>()
            .WithMany()
            .HasForeignKey(module => module.ModuleId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}