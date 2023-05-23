namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Absences;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ResponseConfiguration : IEntityTypeConfiguration<Response>
{
    public void Configure(EntityTypeBuilder<Response> builder)
    {
        builder.ToTable("Absences_Responses");

        builder
            .HasKey(response => response.Id);

        builder
            .Property(response => response.Id)
            .HasConversion(
                id => id.Value,
                value => AbsenceResponseId.FromValue(value));

        builder
            .Property(response => response.AbsenceId)
            .HasConversion(
                id => id.Value,
                value => AbsenceId.FromValue(value));

        builder
            .Property(response => response.Type)
            .HasConversion(
                entry => entry.Value,
                value => ResponseType.FromValue(value));

        builder
            .Property(response => response.VerificationStatus)
            .HasConversion(
                entry => entry.Value,
                value => ResponseVerificationStatus.FromValue(value));

        builder
            .HasOne<Absence>()
            .WithMany()
            .HasForeignKey(response => response.AbsenceId);
    }
}
