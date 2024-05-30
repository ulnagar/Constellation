namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Assets;

using Core.Models.Assets;
using Core.Models.Assets.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes", "Assets");

        builder
            .HasKey(note => note.Id);

        builder
            .Property(note => note.Id)
            .HasConversion(
                id => id.Value,
                value => NoteId.FromValue(value));

        builder
            .Property(note => note.Message)
            .IsRequired();

        builder
            .Property(note => note.AssetId)
            .IsRequired();
    }
}
