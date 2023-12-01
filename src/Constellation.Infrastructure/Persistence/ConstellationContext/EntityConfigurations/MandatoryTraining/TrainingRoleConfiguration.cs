namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.MandatoryTraining;

using Core.Models.MandatoryTraining;
using Core.Models.MandatoryTraining.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingRoleConfiguration : IEntityTypeConfiguration<TrainingRole>
{
    public void Configure(EntityTypeBuilder<TrainingRole> builder)
    {
        builder.ToTable("MandatoryTraining_Roles");

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