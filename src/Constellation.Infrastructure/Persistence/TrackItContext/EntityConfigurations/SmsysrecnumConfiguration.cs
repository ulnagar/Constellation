using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class SmsysrecnumConfiguration : IEntityTypeConfiguration<Models.Index>
    {
        public void Configure(EntityTypeBuilder<Models.Index> builder)
        {
            builder.HasKey(e => e.Name);
            builder.ToTable("SMSYSRECNUM", tb =>
            {
                tb.HasTrigger("ad_SMSYSRECNUM_st");
                tb.HasTrigger("ai_SMSYSRECNUM_st");
                tb.HasTrigger("au_SMSYSRECNUM_st");
            });
            builder.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(128);
            builder.Property(e => e.Lastmodified).HasColumnName("LASTMODIFIED").HasColumnType("datetime").HasDefaultValueSql("(getdate())");
            builder.Property(e => e.Recnum).HasColumnName("RECNUM").HasDefaultValueSql("((1))");
        }
    }

}
