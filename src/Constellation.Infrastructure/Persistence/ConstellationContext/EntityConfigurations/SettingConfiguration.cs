namespace Constellation.Infrastructure.Persistence.ConstellationContext.EntityConfigurations;

using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.HasKey(s => new { s.Name, s.Type });
    }
}