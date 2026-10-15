namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.EnrolmentParent;

using ComplexTypes;
using ConstellationContext.Converters;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

internal sealed class EnrolmentParentConfiguration : IEntityTypeConfiguration<EnrolmentParent>
{
    public void Configure(EntityTypeBuilder<EnrolmentParent> builder)
    {
        builder.ToTable("Parents");

        builder
            .HasKey(entry => entry.Id);

        builder
            .Property(entry => entry.Email)
            .HasConversion<EmailAddressConverter>()
            .IsRequired(false);

        builder
            .Property(entry => entry.PhoneNumber)
            .HasConversion<PhoneNumberConverter>()
            .IsRequired(false);

        builder
            .OwnsOne(
                entry => entry.MailingAddress,
                owned => owned.ConfigureMailingAddress());
    }
}
