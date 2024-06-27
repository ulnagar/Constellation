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
            .Navigation(action => action.Notes)
            .AutoInclude();

        builder
            .HasDiscriminator<string>("ActionType")
            .HasValue<SendEmailAction>(nameof(SendEmailAction))
            .HasValue<PhoneParentAction>(nameof(PhoneParentAction))
            .HasValue<ParentInterviewAction>(nameof(ParentInterviewAction))
            .HasValue<CreateSentralEntryAction>(nameof(CreateSentralEntryAction))
            .HasValue<ConfirmSentralEntryAction>(nameof(ConfirmSentralEntryAction))
            .HasValue<CaseDetailUpdateAction>(nameof(CaseDetailUpdateAction))
            .HasValue<SentralIncidentStatusAction>(nameof(SentralIncidentStatusAction))
            .HasValue<UploadTrainingCertificateAction>(nameof(UploadTrainingCertificateAction));
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

internal sealed class PhoneParentActionConfiguration : IEntityTypeConfiguration<PhoneParentAction>
{
    public void Configure(EntityTypeBuilder<PhoneParentAction> builder)
    {
        builder
            .Property(action => action.ParentName)
            .HasColumnName(nameof(PhoneParentAction.ParentName));

        builder
            .Property(action => action.DateOccurred)
            .HasColumnName(nameof(PhoneParentAction.DateOccurred));

        builder
            .Property(action => action.IncidentNumber)
            .HasColumnName(nameof(PhoneParentAction.IncidentNumber));
    }
}

internal sealed class ParentInterviewActionConfiguration : IEntityTypeConfiguration<ParentInterviewAction>
{
    public void Configure(EntityTypeBuilder<ParentInterviewAction> builder)
    {
        builder
            .HasMany(action => action.Attendees)
            .WithOne()
            .HasForeignKey(attendee => attendee.ActionId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(action => action.Attendees)
            .AutoInclude();

        builder
            .Property(action => action.DateOccurred)
            .HasColumnName(nameof(ParentInterviewAction.DateOccurred));

        builder
            .Property(action => action.IncidentNumber)
            .HasColumnName(nameof(ParentInterviewAction.IncidentNumber));
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

        builder
            .Property(action => action.IncidentNumber)
            .HasColumnName(nameof(CreateSentralEntryAction.IncidentNumber));
    }
}

internal sealed class SentralIncidentStatusActionConfiguration : IEntityTypeConfiguration<SentralIncidentStatusAction>
{
    public void Configure(EntityTypeBuilder<SentralIncidentStatusAction> builder)
    {
        builder
            .Property(action => action.IncidentNumber)
            .HasColumnName(nameof(SentralIncidentStatusAction.IncidentNumber));
    }
}

internal sealed class UploadTrainingCertificateActionConfiguration : IEntityTypeConfiguration<UploadTrainingCertificateAction>
{
    public void Configure(EntityTypeBuilder<UploadTrainingCertificateAction> builder)
    {
        builder
            .Property(action => action.ModuleName)
            .HasColumnName(nameof(UploadTrainingCertificateAction.ModuleName));
    }
}