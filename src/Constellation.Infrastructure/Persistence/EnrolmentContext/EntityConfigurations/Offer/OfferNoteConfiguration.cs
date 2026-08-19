namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.Offer;

using Core.Models.EnrolmentContext.Offer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class OfferNoteConfiguration : IEntityTypeConfiguration<OfferNote>
{
    public void Configure(EntityTypeBuilder<OfferNote> builder)
    {
        builder.ToTable("OfferNotes");

        builder
            .HasKey(entry => entry.Id);
    }
}