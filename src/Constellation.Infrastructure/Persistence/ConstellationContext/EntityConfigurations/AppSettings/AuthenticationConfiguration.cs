namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations.AppSettings;

using Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class AuthenticationConfiguration : IEntityTypeConfiguration<AuthenticationSettings>
{
    public void Configure(EntityTypeBuilder<AuthenticationSettings> builder)
    {
        builder.ToTable("Authentication", "AppSettings");
        
        builder
            .Property<int>("Id");

        builder
            .HasKey("Id");
    }
}