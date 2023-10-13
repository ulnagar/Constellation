namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Attachments;

using Core.Models.Attachments;
using Core.Models.Attachments.Identifiers;
using Core.Models.Attachments.ValueObjects;
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
            .HasConversion(
                type => type.Value,
                value => AttachmentType.FromValue(value));
    }
}
