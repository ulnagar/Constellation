namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Application.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AppAccessTokenConfiguration : IEntityTypeConfiguration<AppAccessToken>
{
    public void Configure(EntityTypeBuilder<AppAccessToken> builder)
    {
        builder.HasKey(a => a.AccessToken);

        builder.Property(a => a.AccessToken)
            .ValueGeneratedNever();
    }
}