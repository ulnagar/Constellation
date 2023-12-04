namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training.Roles;

using Constellation.Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingRoleConfiguration : IEntityTypeConfiguration<TrainingRole>
{
    public void Configure(EntityTypeBuilder<TrainingRole> builder)
    {
        builder.ToTable("Training_Roles_Roles");

        builder
            .HasKey(role => role.Id);

        builder
            .Property(role => role.Id)
            .HasConversion(
                id => id.Value,
                value => TrainingRoleId.FromValue(value));

        builder
            .HasMany(role => role.Members)
            .WithOne(member => member.Role)
            .HasForeignKey(member => member.RoleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasMany(role => role.Modules)
            .WithOne(module => module.Role)
            .HasForeignKey(module => module.RoleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .Navigation(role => role.Members)
            .AutoInclude();

        builder
            .Navigation(role => role.Modules)
            .AutoInclude();
    }
}