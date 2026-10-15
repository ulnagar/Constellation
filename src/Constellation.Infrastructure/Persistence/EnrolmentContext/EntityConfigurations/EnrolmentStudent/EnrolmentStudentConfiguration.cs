namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.EnrolmentStudent;

using ComplexTypes;
using ConstellationContext.Converters;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

internal sealed class EnrolmentStudentConfiguration : IEntityTypeConfiguration<EnrolmentStudent>
{
    public void Configure(EntityTypeBuilder<EnrolmentStudent> builder)
    {
        builder.ToTable("Students");

        builder
            .HasKey(entry => entry.Id);

        builder
            .Property(entry => entry.StudentReferenceNumber)
            .HasConversion<StudentReferenceNumberConverter>()
            .IsRequired(false);

        builder
            .ComplexProperty(entry => entry.Name)
            .IsRequired()
            .ConfigureName();

        builder
            .Property(entry => entry.Gender)
            .IsRequired()
            .HasConversion<GenderConverter>();

        builder
            .Property(entry => entry.DateOfBirth)
            .IsRequired(false);

        builder
            .Property(entry => entry.Email)
            .HasConversion<EmailAddressConverter>()
            .IsRequired(false);

        builder
            .HasMany(entry => entry.Parents)
            .WithMany(entry => entry.Students);
    }
}
