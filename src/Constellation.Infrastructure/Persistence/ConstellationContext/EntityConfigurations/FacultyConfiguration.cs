namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.HasKey(faculty => faculty.Id);

        builder.HasMany(faculty => faculty.Members)
            .WithOne(member => member.Faculty)
            .HasForeignKey(member => member.FacultyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasData(
            new Faculty { Id = Guid.Parse("30cfc9b6-662a-11ed-9022-0242ac120002"), Name = "Administration", Colour = "" },
            new Faculty { Id = Guid.Parse("30cfce98-662a-11ed-9022-0242ac120002"), Name = "Executive", Colour = "" },
            new Faculty { Id = Guid.Parse("30cfd05a-662a-11ed-9022-0242ac120002"), Name = "English", Colour = "" },
            new Faculty { Id = Guid.Parse("30cfd26c-662a-11ed-9022-0242ac120002"), Name = "Mathematics", Colour = "" },
            new Faculty { Id = Guid.Parse("30cfd3de-662a-11ed-9022-0242ac120002"), Name = "Science", Colour = "" },
            new Faculty { Id = Guid.Parse("30cfd51e-662a-11ed-9022-0242ac120002"), Name = "Stage 3", Colour = "" },
            new Faculty { Id = Guid.Parse("30cfda8c-662a-11ed-9022-0242ac120002"), Name = "Support", Colour = "" });
    }
}
