namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.WorkFlows;

using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class ActionNoteConfiguration : IEntityTypeConfiguration<ActionNote>
{
    public void Configure(EntityTypeBuilder<ActionNote> builder)
    {
        builder.ToTable("WorkFlows_ActionNotes");

        builder
            .HasKey(note => note.Id);

        builder
            .Property(note => note.Id)
            .HasConversion(
                id => id.Value,
                value => ActionNoteId.FromValue(value));

        //builder
        //    .Property(note => note.ActionId)
        //    .HasConversion(
        //        id => id.Value,
        //        value => ActionId.FromValue(value));
    }
}
