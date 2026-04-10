namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attachments;

using Core.Models.Attachments;
using Core.Models.Attachments.Enums;
using Core.Models.Attachments.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments_Attachments");

        builder
            .HasKey(attachment => attachment.Id);

        builder
            .Property(attachment => attachment.Id)
            .HasConversion(
                id => id.Value,
                value => AttachmentId.FromValue(value));

        builder
            .Property(attachment => attachment.LinkType)
            .IsRequired()
            .HasConversion(
                type => type.Value,
                value => AttachmentType.FromValue(value));

        builder
            .Property(attachment => attachment.LinkId)
            .IsRequired();

        builder
            .Property(entry => entry.FileData)
            .IsRequired(false);

        builder
            .Property(entry => entry.FilePath)
            .IsRequired(false);
    }
}
