namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.Faculties;

using Constellation.Core.Models.Faculty;
using Core.Models.Faculty.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable("Faculties_Faculty");

        builder
            .HasKey(faculty => faculty.Id);

        builder
            .Property(faculty => faculty.Id)
            .HasConversion(
                id => id.Value,
                value => FacultyId.FromValue(value));
        
        builder
            .HasMany(faculty => faculty.Members)
            .WithOne()
            .HasForeignKey(member => member.FacultyId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
