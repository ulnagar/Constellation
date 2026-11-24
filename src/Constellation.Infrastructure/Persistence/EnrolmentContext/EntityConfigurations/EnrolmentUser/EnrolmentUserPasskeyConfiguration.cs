namespace Constellation.Infrastructure.Persistence.EnrolmentContext.EntityConfigurations.EnrolmentUser;

using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class EnrolmentUserPasskeyConfiguration : IEntityTypeConfiguration<EnrolmentUserPasskey>
{
    public void Configure(EntityTypeBuilder<EnrolmentUserPasskey> builder)
    {
        builder.ToTable("AspNetUserPasskeys");

        builder
            .HasKey(entry => entry.CredentialId);

        builder
            .Property(entry => entry.CredentialId)
            .ValueGeneratedNever();

        builder
            .HasOne(entry => entry.User)
            .WithMany(entry => entry.PasskeyCredentials)
            .HasForeignKey(entry => entry.UserId);

        builder
            .Navigation(entry => entry.User)
            .AutoInclude();
    }
}