using Constellation.Application.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations
{
    public class AppAccessTokenConfiguration : IEntityTypeConfiguration<AppAccessToken>
    {
        public void Configure(EntityTypeBuilder<AppAccessToken> builder)
        {
            builder.HasKey(a => a.AccessToken);

            builder.Property(a => a.AccessToken)
                .ValueGeneratedNever();
        }
    }
}