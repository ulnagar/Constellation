﻿using Constellation.Core.Models.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class OfferingResourceConfiguration : IEntityTypeConfiguration<OfferingResource>
    {
        public void Configure(EntityTypeBuilder<OfferingResource> builder)
        {
            builder.HasKey(r => r.Id);

            builder.HasOne(r => r.Offering)
                .WithMany(o => o.Resources)
                .HasForeignKey(r => r.OfferingId);
        }
    }
}