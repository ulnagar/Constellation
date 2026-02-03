using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class SmsysrecnumConfiguration : IEntityTypeConfiguration<Models.Index>
    {
        public void Configure(EntityTypeBuilder<Models.Index> entity)
        {
            entity.HasKey(e => e.Name);

            entity.ToTable("SMSYSRECNUM", tb =>
            {
                tb.HasTrigger("ad_SMSYSRECNUM_st");
                tb.HasTrigger("ai_SMSYSRECNUM_st");
                tb.HasTrigger("au_SMSYSRECNUM_st");
            });

            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("NAME");
            entity.Property(e => e.Lastmodified)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("LASTMODIFIED");
            entity.Property(e => e.Recnum)
                .HasDefaultValue(1)
                .HasColumnName("RECNUM");
        }
    }

}
