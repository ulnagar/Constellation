namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Tutorials;

using Core.Models.Tutorials;
using Core.Models.Tutorials.Enums;
using Core.Models.Tutorials.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class RequestNoteConfiguration: IEntityTypeConfiguration<RequestNote>
{
    public void Configure(EntityTypeBuilder<RequestNote> builder)
    {
        builder.ToTable("RequestNotes", "Tutorials");

        builder
            .HasKey(note => note.Id);

        builder
            .Property(note => note.Id)
            .HasConversion(
                id => id.Value,
                value => RequestNoteId.FromValue(value));

        builder
            .Property(note => note.Action)
            .HasConversion(
                action => action.Value,
                value => RequestNoteAction.FromValue(value));
    }
}