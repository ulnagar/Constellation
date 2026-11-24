namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.EnrolmentUser;

using Core.Models.Auth.Enums;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EnrolmentUserLoginAttemptConfiguration : IEntityTypeConfiguration<EnrolmentUserLoginAttempt>
{
    public void Configure(EntityTypeBuilder<EnrolmentUserLoginAttempt> builder)
    {
        builder.ToTable("AspNetUserLoginAttempts");

        builder
            .HasKey(attempt => new { attempt.UserId, attempt.LoginDateTime });

        builder
            .Property(attempt => attempt.Status)
            .HasConversion(
                status => status.Value,
                value => LoginStatus.FromValue(value)!);
    }
}