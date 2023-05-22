using Constellation.Core.Models.Absences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class AbsenceResponseConfiguration : IEntityTypeConfiguration<AbsenceResponse>
    {
        public void Configure(EntityTypeBuilder<AbsenceResponse> builder)
        {
            builder.HasKey(response => response.Id);

            builder.Property(response => response.Id).ValueGeneratedOnAdd();

            builder.HasOne(response => response.Absence)
                .WithMany(absence => absence.Responses)
                .HasForeignKey(response => response.AbsenceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
