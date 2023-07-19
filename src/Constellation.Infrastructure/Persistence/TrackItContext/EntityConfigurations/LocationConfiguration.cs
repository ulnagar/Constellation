using Constellation.Infrastructure.Persistence.TrackItContext.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Constellation.Infrastructure.Persistence.TrackItContext.EntityConfigurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.HasKey(e => e.Sequence).HasName("PK_LOCATION_");
            builder.ToTable("_LOCATION_", "_SMDBA_", tb =>
            {
                tb.HasTrigger("sm_ad__LOCATION_st");
                tb.HasTrigger("sm_ai__LOCATION_st");
                tb.HasTrigger("sm_au__LOCATION_st");
            });
            builder.HasIndex(e => new { e.Country, e.Sequence }, "Country");
            builder.HasIndex(e => new { e.SeqCountry, e.Sequence }, "FK_LOCATION_SEQ_COUNTRY");
            builder.HasIndex(e => e.SeqPriority, "FK_LOCATION_SEQ_PRIORITY");
            builder.HasIndex(e => new { e.Group, e.Sequence }, "FK_LOCATION__GROUP_");
            builder.HasIndex(e => new { e.Owner, e.Sequence }, "FK_LOCATION__OWNER_");
            builder.HasIndex(e => e.Name, "UQ_LOCATION_NAME").IsUnique();
            builder.Property(e => e.Sequence).ValueGeneratedNever().HasColumnName("SEQUENCE");
            builder.Property(e => e.Address).HasMaxLength(128).HasColumnName("ADDRESS");
            builder.Property(e => e.City).HasMaxLength(20).HasColumnName("CITY");
            builder.Property(e => e.Code).HasMaxLength(15).HasColumnName("CODE");
            builder.Property(e => e.Comments).HasMaxLength(255).HasColumnName("COMMENTS");
            builder.Property(e => e.Country).HasMaxLength(25).HasColumnName("COUNTRY");
            builder.Property(e => e.Fax).HasMaxLength(13).HasColumnName("FAX");
            builder.Property(e => e.Group).HasColumnName("_GROUP_");
            builder.Property(e => e.Inactive).HasColumnName("_INACTIVE_:");
            builder.Property(e => e.IntlFax).HasMaxLength(16).HasColumnName("INTL_FAX_#").IsFixedLength(true);
            builder.Property(e => e.IntlPhone).HasMaxLength(30).HasColumnName("INTL_PHONE_#").IsFixedLength(true);
            builder.Property(e => e.IntlPostCode).HasMaxLength(9).HasColumnName("INTL_POST_CODE").IsFixedLength(true);
            builder.Property(e => e.Lastmodified).HasColumnType("datetime").HasColumnName("LASTMODIFIED").HasDefaultValueSql("(getdate())");
            builder.Property(e => e.Lastuser).HasMaxLength(255).HasColumnName("LASTUSER").HasDefaultValueSql("(user_name())");
            builder.Property(e => e.MainContact).HasMaxLength(50).HasColumnName("MAIN_CONTACT");
            builder.Property(e => e.Maincontctphone).HasMaxLength(30).HasColumnName("MAINCONTCTPHONE");
            builder.Property(e => e.Name).IsRequired().HasMaxLength(50).HasColumnName("NAME");
            builder.Property(e => e.Note).HasColumnName("NOTE");
            builder.Property(e => e.Owner).HasColumnName("_OWNER_");
            builder.Property(e => e.Ownerperms).HasColumnName("_OWNERPERMS_");
            builder.Property(e => e.Phone).HasMaxLength(30).HasColumnName("PHONE");
            builder.Property(e => e.SeqCountry).HasColumnName("SEQ_COUNTRY");
            builder.Property(e => e.SeqPriority).HasColumnName("SEQ_PRIORITY");
            builder.Property(e => e.State).HasMaxLength(25).HasColumnName("STATE");
            builder.Property(e => e.TimeZone).HasMaxLength(10).HasColumnName("TIME_ZONE");
            builder.Property(e => e.Zip).HasMaxLength(10).HasColumnName("ZIP");
        }
    }

}
