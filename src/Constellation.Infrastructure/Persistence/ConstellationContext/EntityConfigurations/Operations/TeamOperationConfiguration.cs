namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Operations;

using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class TeamOperationConfiguration : IEntityTypeConfiguration<TeamOperation>
{
    public void Configure(EntityTypeBuilder<TeamOperation> builder)
    {
        builder.ToTable("Teams", "Operations");

        builder
            .Property(operation => operation.Id)
            .UseSequence(AppDbContext.TeamsOperationId);

        builder
            .HasDiscriminator<string>("OperationType")
            .HasValue<CreateTeamTeamOperation>(nameof(CreateTeamTeamOperation))
            .HasValue<CreateTeamChannelTeamOperation>(nameof(CreateTeamChannelTeamOperation))
            .HasValue<ModifyTeamMembershipTeamOperation>(nameof(ModifyTeamMembershipTeamOperation))
            .HasValue<ModifyTeamChannelMembershipTeamOperation>(nameof(ModifyTeamChannelMembershipTeamOperation))
            .HasValue<ArchiveTeamTeamOperation>(nameof(ArchiveTeamTeamOperation))
            .HasValue<ArchiveTeamChannelTeamOperation>(nameof(ArchiveTeamChannelTeamOperation));
    }
}

internal sealed class CreateTeamTeamOperationConfiguration : IEntityTypeConfiguration<CreateTeamTeamOperation>
{
    public void Configure(EntityTypeBuilder<CreateTeamTeamOperation> builder)
    {
        builder
            .Property(operation => operation.Name)
            .HasColumnName(nameof(CreateTeamTeamOperation.Name));
    }
}

internal sealed class CreateTeamChannelTeamOperationConfiguration : IEntityTypeConfiguration<CreateTeamChannelTeamOperation>
{
    public void Configure(EntityTypeBuilder<CreateTeamChannelTeamOperation> builder)
    {
        builder
            .Property(operation => operation.TeamId)
            .HasColumnName(nameof(CreateTeamChannelTeamOperation.TeamId));


        builder
            .Property(operation => operation.Name)
            .HasColumnName(nameof(CreateTeamChannelTeamOperation.Name));
    }
}

internal sealed class ModifyTeamMembershipTeamOperationConfiguration : IEntityTypeConfiguration<ModifyTeamMembershipTeamOperation>
{
    public void Configure(EntityTypeBuilder<ModifyTeamMembershipTeamOperation> builder)
    {
        builder
            .Property(operation => operation.TeamId)
            .HasColumnName(nameof(ModifyTeamMembershipTeamOperation.TeamId));

        builder
            .Property(operation => operation.UserId)
            .HasColumnName(nameof(ModifyTeamMembershipTeamOperation.UserId))
            .HasConversion(
                email => email.Email,
                value => EmailAddress.FromValue(value));

        builder
            .Property(operation => operation.Action)
            .HasColumnName(nameof(ModifyTeamMembershipTeamOperation.Action))
            .HasConversion(
                action => action.Value,
                value => TeamAction.FromValue(value));
    }
}

internal sealed class ModifyTeamChannelMembershipTeamOperationConfiguration : IEntityTypeConfiguration<ModifyTeamChannelMembershipTeamOperation>
{
    public void Configure(EntityTypeBuilder<ModifyTeamChannelMembershipTeamOperation> builder)
    {
        builder
            .Property(operation => operation.TeamId)
            .HasColumnName(nameof(ModifyTeamChannelMembershipTeamOperation.TeamId));

        builder
            .Property(operation => operation.UserId)
            .HasColumnName(nameof(ModifyTeamChannelMembershipTeamOperation.UserId))
            .HasConversion(
                email => email.Email,
                value => EmailAddress.FromValue(value));

        builder
            .Property(operation => operation.Action)
            .HasColumnName(nameof(ModifyTeamChannelMembershipTeamOperation.Action))
            .HasConversion(
                action => action.Value,
                value => TeamAction.FromValue(value));

        builder
            .Property(operation => operation.ChannelName)
            .HasColumnName(nameof(ModifyTeamChannelMembershipTeamOperation.ChannelName));
    }
}

internal sealed class ArchiveTeamTeamOperationConfiguration : IEntityTypeConfiguration<ArchiveTeamTeamOperation>
{
    public void Configure(EntityTypeBuilder<ArchiveTeamTeamOperation> builder)
    {
        builder
            .Property(operation => operation.TeamId)
            .HasColumnName(nameof(ArchiveTeamTeamOperation.TeamId));
    }
}

internal sealed class ArchiveTeamChannelTeamOperationConfiguration : IEntityTypeConfiguration<ArchiveTeamChannelTeamOperation>
{
    public void Configure(EntityTypeBuilder<ArchiveTeamChannelTeamOperation> builder)
    {
        builder
            .Property(operation => operation.TeamId)
            .HasColumnName(nameof(ArchiveTeamChannelTeamOperation.TeamId));

        builder
            .Property(operation => operation.ChannelName)
            .HasColumnName(nameof(ArchiveTeamChannelTeamOperation.ChannelName));
    }
}