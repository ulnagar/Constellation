using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class SmsysrecnumConfiguration : IEntityTypeConfiguration<Index>
    {
        public void Configure(EntityTypeBuilder<Index> builder)
        {
            builder.HasKey(e => e.Name);
            builder.ToTable("SMSYSRECNUM");
            builder.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(128);
            builder.Property(e => e.Lastmodified).HasColumnName("LASTMODIFIED").HasColumnType("datetime").HasDefaultValueSql("(getdate())");
            builder.Property(e => e.Recnum).HasColumnName("RECNUM").HasDefaultValueSql("((1))");
        }
    }

}
