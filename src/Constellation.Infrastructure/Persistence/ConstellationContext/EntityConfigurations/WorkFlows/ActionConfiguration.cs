namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.WorkFlows;

using Core.Models;
using Core.Models.Offerings;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Identifiers;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ActionConfiguration : IEntityTypeConfiguration<Action>
{
    public void Configure(EntityTypeBuilder<Action> builder)
    {
        builder.ToTable("WorkFlows_Actions");

        builder
            .HasKey(action => action.Id);

        builder
            .Property(action => action.Id)
            .HasConversion(
                id => id.Value,
                value => ActionId.FromValue(value));

        builder
            .Property(action => action.CaseId)
            .HasConversion(
                id => id.Value,
                value => CaseId.FromValue(value));

        builder
            .Property(action => action.ParentActionId)
            .HasConversion(
                id => id.Value,
                value => ActionId.FromValue(value));

        builder
            .Property(action => action.Status)
            .HasConversion(
                status => status.Value,
                value => ActionStatus.FromValue(value));

        builder
            .HasMany(action => action.Notes)
            .WithOne()
            .HasForeignKey(note => note.ActionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Staff>()
            .WithMany()
            .HasForeignKey(action => action.AssignedToId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder
            .HasDiscriminator<string>("ActionType")
            .HasValue<SendEmailAction>(nameof(SendEmailAction))
            .HasValue<PhoneParentAction>(nameof(PhoneParentAction))
            .HasValue<ParentInterviewAction>(nameof(ParentInterviewAction))
            .HasValue<CreateSentralEntryAction>(nameof(CreateSentralEntryAction))
            .HasValue<ConfirmSentralEntryAction>(nameof(ConfirmSentralEntryAction));
    }
}

internal sealed class SendEmailActionConfiguration : IEntityTypeConfiguration<SendEmailAction>
{
    public void Configure(EntityTypeBuilder<SendEmailAction> builder)
    {
        builder
            .OwnsOne(action => action.Sender,
                config =>
                {
                    config
                        .Property(sender => sender.Name)
                        .HasColumnName("SenderName");

                    config
                        .Property(sender => sender.Email)
                        .HasColumnName("SenderEmail");
                });

        builder
            .OwnsMany(
                action => action.Recipients,
                config =>
                {
                    config.ToTable("WorkFlows_Actions_Recipients");

                    config
                        .WithOwner()
                        .HasForeignKey("OwnerId");

                    config
                        .Property<int>("Id");

                    config
                        .HasKey("Id");

                    config
                        .Property(recipient => recipient.Email)
                        .HasColumnName(nameof(EmailRecipient.Email));

                    config
                        .Property(recipient => recipient.Name)
                        .HasColumnName(nameof(EmailRecipient.Name));
                });
    }
}

internal sealed class CreateSentralEntryActionConfiguration : IEntityTypeConfiguration<CreateSentralEntryAction>
{
    public void Configure(EntityTypeBuilder<CreateSentralEntryAction> builder)
    {
        builder
            .HasOne<Offering>()
            .WithMany()
            .HasForeignKey(action => action.OfferingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}