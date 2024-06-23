namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Training.Roles;

using Constellation.Core.Models.Training;
using Core.Models;
using Core.Models.Training.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TrainingRoleMemberConfiguration : IEntityTypeConfiguration<TrainingRoleMember>
{
    public void Configure(EntityTypeBuilder<TrainingRoleMember> builder)
    {
        builder.ToTable("Training_Roles_Members");

        builder
            .HasKey(member => new { member.RoleId, member.StaffId });

        builder
            .Property(member => member.RoleId)
            .HasConversion(
                id => id.Value,
                value => TrainingRoleId.FromValue(value));

        builder
            .HasOne(member => member.Role)
            .WithMany(role => role.Members)
            .HasForeignKey(member => member.RoleId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(member => member.StaffId)
            .OnDelete(DeleteBehavior.ClientCascade);
    }
}